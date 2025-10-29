using Catalog.Api.Dtos;
using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Catalog.Domain.Enums;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly CatalogDbContext _context;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(
        IVehicleRepository vehicleRepository,
        CatalogDbContext context,
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<VehiclesController> logger)
    {
        _vehicleRepository = vehicleRepository;
        _context = context;
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
    }

    // GET: api/vehicles?pageNumber=1&pageSize=8
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 8)
    {
        var pagedResult = await _vehicleRepository.GetAllAsync(pageNumber, pageSize);
        return Ok(pagedResult);
    }

    // GET: api/vehicles/GUID (Público)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null)
        {
            return NotFound();
        }
        return Ok(vehicle);
    }

    // POST: api/vehicles (Protegido)
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] VehicleDto request)
    {
        try
        {
            var vehicle = new Vehicle(
                request.Brand,
                request.Model,
                request.Year.Value,
                request.Price,
                request.Mileage.Value,
                request.Color,
                request.Description,
                request.Transmission.Value,
                request.VehicleType.Value,
                request.FuelType.Value
            );

            // Agregar Features
            if (request.FeatureIds != null && request.FeatureIds.Any())
            {
                var features = await _context.Features
                    .Where(f => request.FeatureIds.Contains(f.Id))
                    .ToListAsync();

                foreach (var feature in features)
                {
                    vehicle.Features.Add(feature);
                }
            }

            // ✅ Guardar el vehículo primero para obtener su Id
            await _vehicleRepository.AddAsync(vehicle);

            // ✅ Ahora agregar las imágenes con el VehicleId
            if (request.ImageUrls != null && request.ImageUrls.Any())
            {
                var images = request.ImageUrls
                    .Select((url, index) => new Image(url, index, vehicle.Id))
                    .ToList();

                await _context.Images.AddRangeAsync(images);
                await _context.SaveChangesAsync();
            }

            var redis = _connectionMultiplexer.GetSubscriber();
            await redis.PublishAsync("vehicles", vehicle.Id.ToString());

            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError($"Error de validación en Create: {ex.Message}");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error en Create: {ex.Message}");
            return StatusCode(500, "Error al crear el vehículo");
        }
    }

    // PUT: api/vehicles/GUID
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] VehicleDto request)
    {
        try
        {
            // 1. Cargar vehículo SIN imágenes (evitar problemas de tracking)
            var vehicleToUpdate = await _context.Vehicles
                .Include(v => v.Features)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicleToUpdate == null)
            {
                return NotFound("Vehículo no encontrado.");
            }

            if (vehicleToUpdate.Status == VehicleStatus.Sold)
            {
                return BadRequest("No se puede modificar un vehículo que ya ha sido vendido.");
            }

            // 2. Actualizar propiedades simples
            vehicleToUpdate.UpdateDetails(
                request.Brand,
                request.Model,
                request.Year.Value,
                request.Price,
                request.Mileage.Value,
                request.Color,
                request.Description,
                request.Transmission.Value,
                request.VehicleType.Value,
                request.FuelType.Value
            );
            vehicleToUpdate.SetCoverImage(request.CoverImageUrl);

            // 3. Eliminar imágenes antiguas (consulta separada)
            var oldImages = await _context.Images
                .Where(i => i.VehicleId == id)
                .ToListAsync();

            _context.Images.RemoveRange(oldImages);

            // 4. Agregar nuevas imágenes con VehicleId
            if (request.ImageUrls != null && request.ImageUrls.Any())
            {
                var newImages = request.ImageUrls
                    .Select((url, index) => new Image(url, index, id))
                    .ToList();

                await _context.Images.AddRangeAsync(newImages);
            }

            // 5. Actualizar Features (muchos-a-muchos)
            vehicleToUpdate.Features.Clear();
            if (request.FeatureIds != null && request.FeatureIds.Any())
            {
                var features = await _context.Features
                    .Where(f => request.FeatureIds.Contains(f.Id))
                    .ToListAsync();

                foreach (var feature in features)
                {
                    vehicleToUpdate.Features.Add(feature);
                }
            }

            await _context.SaveChangesAsync();

            var redis = _connectionMultiplexer.GetSubscriber();
            await redis.PublishAsync("vehicles", vehicleToUpdate.Id.ToString());

            return NoContent();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError($"Error de concurrencia en Update: {ex.Message}");
            return Conflict("El vehículo fue modificado por otro usuario.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError($"Error de validación en Update: {ex.Message}");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error en Update: {ex.Message}");
            return StatusCode(500, "Error al actualizar el vehículo");
        }
    }

    // PUT: api/vehicles/GUID/price
    [HttpPut("{id}/price")]
    [Authorize]
    public async Task<IActionResult> UpdatePrice(Guid id, [FromBody] UpdatePriceDto request)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null) return NotFound();

        try
        {
            vehicle.UpdatePrice(request.NewPrice);
            await _vehicleRepository.UpdateAsync(vehicle);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: api/vehicles/GUID/sell
    [HttpPost("{id}/sell")]
    [Authorize]
    public async Task<IActionResult> MarkAsSold(Guid id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null) return NotFound();

        try
        {
            vehicle.MarkAsSold();
            await _vehicleRepository.UpdateAsync(vehicle);
            return Ok(vehicle);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: api/vehicles/{vehicleId}/features
    [HttpPost("{vehicleId:guid}/features")]
    [Authorize]
    public async Task<IActionResult> AddFeatureToVehicle(Guid vehicleId, [FromBody] AddFeatureDto dto)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
        if (vehicle is null) return NotFound("Vehículo no encontrado.");

        var feature = await _context.Features.FindAsync(dto.FeatureId);
        if (feature is null) return NotFound("Equipamiento no encontrado.");

        if (vehicle.Features.Any(f => f.Id == dto.FeatureId))
        {
            return BadRequest("El vehículo ya tiene este equipamiento.");
        }

        vehicle.Features.Add(feature);
        await _vehicleRepository.UpdateAsync(vehicle);

        return Ok(vehicle);
    }

    // DELETE: api/vehicles/{id}
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var vehicleToDelete = await _vehicleRepository.GetByIdAsync(id);
        if (vehicleToDelete == null)
        {
            return NotFound();
        }

        await _vehicleRepository.DeleteAsync(vehicleToDelete);

        // Notificamos a Redis que un vehículo fue eliminado
        // (El Search Service necesitará manejar este evento)
        var redis = _connectionMultiplexer.GetSubscriber();
        await redis.PublishAsync("vehicles_deleted", id.ToString());

        return NoContent(); // Éxito
    }
    // POST: api/vehicles/{id}/reserve
    [HttpPost("{id:guid}/reserve")]
    [Authorize]
    public async Task<IActionResult> Reserve(Guid id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null) return NotFound();

        try
        {
            vehicle.Reserve();
            await _vehicleRepository.UpdateAsync(vehicle);
            return Ok(vehicle);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: api/vehicles/{id}/make-available
    [HttpPost("{id:guid}/make-available")]
    [Authorize]
    public async Task<IActionResult> MakeAvailable(Guid id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null) return NotFound();

        vehicle.MakeAvailable();
        await _vehicleRepository.UpdateAsync(vehicle);
        return Ok(vehicle);
    }

    // GET: api/vehicles/latest
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest()
    {
        // Devolvemos los últimos 4 vehículos
        var latestVehicles = await _vehicleRepository.GetLatestAsync(4);
        return Ok(latestVehicles);
    }

    // NUEVO ENDPOINT: GET /api/vehicles/featured
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
    {
        var featuredVehicles = await _context.Vehicles
            .Where(v => v.IsFeatured && v.Status == VehicleStatus.Available)
            .Include(v => v.Images)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
        return Ok(featuredVehicles);
    }

    //NUEVO ENDPOINT: POST /api/vehicles/{id}/toggle-featured
    [HttpPost("{id:guid}/toggle-featured")]
    [Authorize] // Protegido para que solo el admin pueda usarlo
    public async Task<IActionResult> ToggleFeatured(Guid id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null) return NotFound();

        vehicle.ToggleFeaturedStatus();
        await _vehicleRepository.UpdateAsync(vehicle);

        // Notificamos a Redis para que el Search Service actualice el índice
        var redis = _connectionMultiplexer.GetSubscriber();
        await redis.PublishAsync("vehicles", id.ToString());

        return Ok(vehicle);
    }
}
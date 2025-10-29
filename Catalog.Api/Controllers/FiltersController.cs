using Catalog.Domain.Enums;
using Catalog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FiltersController : ControllerBase
{
    private readonly CatalogDbContext _context;
    private readonly ILogger<FiltersController> _logger;

    public FiltersController(CatalogDbContext context, ILogger<FiltersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/filters/brands
    [HttpGet("brands")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBrands()
    {
        _logger.LogInformation("GetBrands endpoint called");
        var brands = await _context.Vehicles
            .Select(v => v.Brand)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync();
        _logger.LogInformation($"Returning {brands.Count} brands");
        return Ok(brands);
    }

    // GET: api/filters/vehicle-types
    [HttpGet("vehicle-types")]
    [AllowAnonymous]
    public IActionResult GetVehicleTypes()
    {
        _logger.LogInformation("GetVehicleTypes endpoint called");
        var types = Enum.GetNames(typeof(VehicleType)).ToList();
        _logger.LogInformation($"Returning {types.Count} vehicle types");
        return Ok(types);
    }

    //GET: api/filters/years
    [HttpGet("years")]
    public async Task<IActionResult> GetYears()
    {
        var years = await _context.Vehicles
            .Select(v => v.Year)
            .Distinct()
            .OrderByDescending(y => y) // Ordenamos del más nuevo al más viejo
            .ToListAsync();
        return Ok(years);
    }
}
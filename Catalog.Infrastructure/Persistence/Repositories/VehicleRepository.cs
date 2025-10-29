using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Dtos; // <-- Make sure this using is present
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly CatalogDbContext _context;

    public VehicleRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Vehicle?> GetByIdAsync(Guid id)
    {
        return await _context.Vehicles
            .Include(v => v.Features)
            .Include(v => v.Images)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    // ✅ MÉTODO ACTUALIZADO PARA COINCIDIR CON LA INTERFAZ Y PAGINAR
    public async Task<PagedResult<Vehicle>> GetAllAsync(int pageNumber, int pageSize)
    {
        var totalCount = await _context.Vehicles.CountAsync();
        var items = await _context.Vehicles
            .Include(v => v.Features)
            .Include(v => v.Images)
            .OrderByDescending(v => v.CreatedAt) // Order to show the newest first
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Vehicle>(items, totalCount, pageNumber, pageSize);
    }

    public async Task AddAsync(Vehicle vehicle)
    {
        await _context.Vehicles.AddAsync(vehicle);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Vehicle vehicle)
    {
        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Vehicle>> GetLatestAsync(int count)
    {
        return await _context.Vehicles
            .Include(v => v.Features)
            .Include(v => v.Images)
            .OrderByDescending(v => v.CreatedAt) // Ordena por fecha de creación, los más nuevos primero
            .Take(count) // Toma solo la cantidad que necesitamos (ej: 4)
            .ToListAsync();
    }
}
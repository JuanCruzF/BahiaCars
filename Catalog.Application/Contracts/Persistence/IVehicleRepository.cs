using Catalog.Application.Dtos;
using Catalog.Domain.Entities;

namespace Catalog.Application.Contracts.Persistence;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id);
    Task<PagedResult<Vehicle>> GetAllAsync(int pageNumber, int pageSize);
    Task AddAsync(Vehicle vehicle);
    Task UpdateAsync(Vehicle vehicle);
    Task DeleteAsync(Vehicle vehicle);
    Task<IReadOnlyList<Vehicle>> GetLatestAsync(int count);
}
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<Image> Images { get; set; }
}
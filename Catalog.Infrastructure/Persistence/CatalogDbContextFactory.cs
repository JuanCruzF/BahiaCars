using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Catalog.Infrastructure.Persistence;

public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        // 1. Construimos un IConfiguration para poder leer el appsettings.json
        // Busca el archivo appsettings.json en el proyecto de la API
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Catalog.Api"))
            .AddJsonFile("appsettings.json")
            .Build();

        // 2. Leemos la cadena de conexión del archivo
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // 3. Creamos el DbContextOptionsBuilder y le pasamos la cadena de conexión
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
        optionsBuilder.UseSqlite(connectionString);

        // 4. Creamos y devolvemos la instancia del DbContext
        return new CatalogDbContext(optionsBuilder.Options);
    }
}
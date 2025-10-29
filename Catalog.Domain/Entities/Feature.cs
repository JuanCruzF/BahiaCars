namespace Catalog.Domain.Entities;

public class Feature
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } // Ej: "ABS", "Control de Estabilidad", "Techo Solar"
    public string Category { get; private set; } // Ej: "Seguridad", "Confort", "Exterior"

    private Feature() { } // Para EF Core

    public Feature(string name, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del equipamiento no puede estar vacío.", nameof(name));

        Id = Guid.NewGuid();
        Name = name;
        Category = category ?? "General";
    }
}
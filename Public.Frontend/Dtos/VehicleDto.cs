namespace Public.Frontend.Dtos;

public class VehicleResponse
{
    public Guid Id { get; set; }
    public string Brand { get; set; } = "";
    public string Model { get; set; } = "";
    public int Year { get; set; }
    public decimal? Price { get; set; }
    public int Mileage { get; set; }
    public string Color { get; set; } = "";
    public string Description { get; set; } = "";
    public string? CoverImageUrl { get; set; }
    public bool IsFeatured { get; set; }

    // Propiedades de tipo Enum
    public TransmissionType Transmission { get; set; }
    public VehicleType VehicleType { get; set; } //PROPIEDAD AÑADIDA
    public FuelType FuelType { get; set; }

    // Colecciones
    public List<ImageResponse> Images { get; set; } = [];
    public List<FeatureResponse> Features { get; set; } = [];
}

public class ImageResponse
{
    public string Url { get; set; } = "";
    public int Position { get; set; }
}

public class FeatureResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
}

// ENUMS DEFINIDOS FUERA DE LAS CLASES, EN EL NAMESPACE
public enum VehicleType
{
    Auto,
    Pickup,
    SUV,
    Utilitario,
    Furgon,
    Moto,
    Cuatriciclo,
    Camioneta
}

public enum TransmissionType
{
    Manual,
    Automatic
}

public enum FuelType
{
    Nafta,
    Diesel,
    GNC,
    Hibrido
}
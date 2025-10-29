using Microsoft.VisualBasic.FileIO;

namespace Admin.Frontend.Dtos;

public class VehicleResponse
{
    public Guid Id { get; set; }
    public string Brand { get; set; } = "";
    public string Model { get; set; } = "";
    public int Year { get; set; }
    public decimal? Price { get; set; }
    public int Mileage { get; set; }
    public VehicleStatus Status { get; set; }
    public string Color { get; set; } = "";
    public string Description { get; set; } = "";
    public TransmissionType Transmission { get; set; }

    public VehicleType VehicleType { get; set; }

    public FuelType FuelType { get; set; }

    //AGREGAR ESTA LÍNEA
    public List<ImageResponse> Images { get; set; } = [];

    public string? ImageUrl { get; set; } // Mantener por compatibilidad
    public List<FeatureResponse> Features { get; set; } = [];
    public bool IsFeatured { get; set; }
}

public class FeatureResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
}

public enum VehicleStatus
{
    Available,
    Reserved,
    Sold
}

public enum TransmissionType
{
    Manual,
    Automatic
}

public class ImageResponse
{
    public string Url { get; set; } = "";
    public int Position { get; set; }
}

public enum VehicleType
{
    Auto,
    Pickup,
    SUV,
    Utilitario,
    Furgon,
    Moto,
    Cuatriciclo
}

public enum FuelType
{
    Nafta,
    Diesel,
    GNC,
    Hibrido
}
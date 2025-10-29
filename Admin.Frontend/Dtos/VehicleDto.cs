namespace Admin.Frontend.Dtos;

// Este es el DTO que se ENVÍA a la API al crear un vehículo
public record VehicleDto(
    string Brand,
    string Model,
    int Year,
    decimal? Price,
    int Mileage,
    string Color,
    string? Description,
    List<Guid> FeatureIds,
    string? CoverImageUrl,
    List<string> ImageUrls,
    TransmissionType Transmission,
    VehicleType VehicleType,
    FuelType FuelType
);
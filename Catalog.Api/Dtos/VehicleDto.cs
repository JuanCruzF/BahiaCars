using Catalog.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Catalog.Api.Dtos;

public record VehicleDto(
    [Required(ErrorMessage = "La marca es obligatoria.")]
    string Brand,

    [Required(ErrorMessage = "El modelo es obligatorio.")]
    string Model,

    [Required(ErrorMessage = "El año es obligatorio.")]
    int? Year,

    decimal? Price,

    [Required(ErrorMessage = "El kilometraje es obligatorio.")]
    int? Mileage,

    [Required(ErrorMessage = "El color es obligatorio.")]
    string Color,

    string? Description,

    string? CoverImageUrl,

    List<Guid>? FeatureIds,

    [MinLength(1, ErrorMessage = "Se requiere al menos una imagen.")]
    List<string>? ImageUrls,

    [Required(ErrorMessage = "El tipo de transmisión es obligatorio.")]
    TransmissionType? Transmission,

    [Required(ErrorMessage = "El tipo de vehículo es obligatorio.")]
    VehicleType? VehicleType,

    [Required(ErrorMessage = "El tipo de combustible es obligatorio.")]
    FuelType? FuelType
);
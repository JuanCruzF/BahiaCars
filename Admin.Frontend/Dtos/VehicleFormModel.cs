using System.ComponentModel.DataAnnotations;
using Admin.Frontend.Dtos;

namespace Admin.Frontend.Dtos;

public class VehicleFormModel
{
    [Required(ErrorMessage = "La marca es obligatoria.")]
    public string? Brand { get; set; }

    [Required(ErrorMessage = "El modelo es obligatorio.")]
    public string? Model { get; set; }

    [Required(ErrorMessage = "El año es obligatorio.")]
    [Range(1900, 2100, ErrorMessage = "El año no es válido.")]
    public int? Year { get; set; }

    public decimal? Price { get; set; }

    [Required(ErrorMessage = "El kilometraje es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El kilometraje no puede ser negativo.")]
    public int? Mileage { get; set; }

    [Required(ErrorMessage = "El color es obligatorio.")]
    public string? Color { get; set; }

    public string? Description { get; set; }

    [Required(ErrorMessage = "El tipo de transmisión es obligatorio.")]
    public TransmissionType? Transmission { get; set; }

    [Required(ErrorMessage = "El tipo de vehículo es obligatorio.")]
    public VehicleType? VehicleType { get; set; }

    [Required(ErrorMessage = "El tipo de combustible es obligatorio.")]
    public FuelType? FuelType { get; set; }
}
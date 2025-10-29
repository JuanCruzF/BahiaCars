using Catalog.Domain.Enums;
using Microsoft.VisualBasic.FileIO;

namespace Catalog.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; private set; }
    public string Brand { get; private set; }
    public string Model { get; private set; }
    public int Year { get; private set; }
    public decimal? Price { get; private set; }
    public int Mileage { get; private set; }
    public string Color { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public VehicleStatus Status { get; private set; }
    public TransmissionType Transmission { get; private set; }
    public VehicleType VehicleType { get; private set; }
    public FuelType FuelType { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public ICollection<Image> Images { get; private set; } = new List<Image>();
    public ICollection<Feature> Features { get; private set; } = new List<Feature>();
    public bool IsFeatured { get; private set; }

    // ✅ CONSTRUCTOR PRIVADO CORREGIDO PARA EVITAR WARNINGS
    private Vehicle()
    {
        Brand = string.Empty;
        Model = string.Empty;
        Color = string.Empty;
        Description = string.Empty;
    }

    public Vehicle(string brand, string model, int year, decimal? price, int mileage, string color, string description, TransmissionType transmission, VehicleType vehicleType, FuelType fuelType)
    {
        if (string.IsNullOrWhiteSpace(brand)) throw new ArgumentException("La marca no puede estar vacía.", nameof(brand));
        if (string.IsNullOrWhiteSpace(model)) throw new ArgumentException("El modelo no puede estar vacío.", nameof(model));
        if (year <= 1900 || year > DateTime.UtcNow.Year + 1) throw new ArgumentException("El año no es válido.", nameof(year));
        if (price.HasValue && price <= 0) throw new ArgumentException("El precio debe ser mayor a cero.", nameof(price));
        if (mileage < 0) throw new ArgumentException("El kilometraje no puede ser negativo.", nameof(mileage));

        Id = Guid.NewGuid();
        Brand = brand;
        Model = model;
        Year = year;
        Price = price;
        Mileage = mileage;
        Color = color ?? string.Empty;
        Description = description ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
        Status = VehicleStatus.Available;
        Transmission = transmission;
        VehicleType = vehicleType;
        FuelType = fuelType;
        IsFeatured = false;
    }

    public void UpdateDetails(string brand, string model, int year, decimal? price, int mileage, string color, string description, TransmissionType transmission, VehicleType vehicleType, FuelType fuelType)
    {
        if (string.IsNullOrWhiteSpace(brand)) throw new ArgumentException("La marca no puede estar vacía.", nameof(brand));
        if (string.IsNullOrWhiteSpace(model)) throw new ArgumentException("El modelo no puede estar vacío.", nameof(model));

        Brand = brand;
        Model = model;
        Year = year;
        Price = price;
        Mileage = mileage;
        Color = color ?? string.Empty;
        Description = description ?? string.Empty;
        Transmission = transmission;
        VehicleType = vehicleType;
        FuelType = fuelType;
    }

    public void SetCoverImage(string? imageUrl)
    {
        CoverImageUrl = imageUrl;
    }

    public void UpdatePrice(decimal? newPrice)
    {
        if (newPrice.HasValue && newPrice <= 0) throw new ArgumentException("El nuevo precio debe ser mayor a cero.", nameof(newPrice));
        Price = newPrice;
    }

    public void MarkAsSold()
    {
        if (Status == VehicleStatus.Sold) throw new InvalidOperationException("Este vehículo ya ha sido vendido.");
        Status = VehicleStatus.Sold;
    }

    public void Reserve()
    {
        if (Status != VehicleStatus.Available) throw new InvalidOperationException("Solo se puede reservar un vehículo que está disponible.");
        Status = VehicleStatus.Reserved;
    }

    public void MakeAvailable()
    {
        if (Status == VehicleStatus.Reserved || Status == VehicleStatus.Sold)
        {
            Status = VehicleStatus.Available;
        }
    }
    public void ToggleFeaturedStatus()
    {
        IsFeatured = !IsFeatured;
    }
}
namespace Catalog.Domain.Entities;

public class Image
{
    public Guid Id { get; private set; }
    public string Url { get; private set; }
    public int Position { get; private set; }
    public Guid VehicleId { get; private set; }
    public Vehicle Vehicle { get; private set; } = null!;

    // Constructor privado para EF Core
    private Image() { }

    // ✅ UN SOLO constructor público - VehicleId es OBLIGATORIO
    public Image(string url, int position, Guid vehicleId)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("La URL de la imagen no puede estar vacía", nameof(url));

        if (vehicleId == Guid.Empty)
            throw new ArgumentException("VehicleId no puede estar vacío", nameof(vehicleId));

        Id = Guid.NewGuid();
        Url = url;
        Position = position;
        VehicleId = vehicleId;
    }
}
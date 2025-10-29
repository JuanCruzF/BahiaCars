namespace Search.Service.Dtos;

public class VehicleSearchResponse
{
    public Guid Id { get; set; }
    public string Brand { get; set; } = "";
    public string Model { get; set; } = "";
    public int Year { get; set; }
    public decimal? Price { get; set; }
    public int Mileage { get; set; }
    public int Status { get; set; }
    public string? CoverImageUrl { get; set; }
    public List<ImageSearchResponse> Images { get; set; } = [];
    public int VehicleType { get; set; }
    public List<FeatureSearchResponse> Features { get; set; } = [];
}

public class ImageSearchResponse
{
    public string Url { get; set; } = "";
    public int Position { get; set; }
}

public class FeatureSearchResponse
{
    public string Name { get; set; } = "";
}
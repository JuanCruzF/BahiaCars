namespace Search.Service.Documents
{
    public record VehicleDocument
    {
        public Guid Id { get; init; }
        public required string Brand { get; init; }
        public required string Model { get; init; }
        public int Year { get; init; }
        public decimal? Price { get; init; }
        public int VehicleType { get; init; }
        public List<string> Features { get; init; } = [];
        public int Mileage { get; init; }
        public string? CoverImageUrl { get; init; }
        public int Status { get; init; }
        public List<ImageDocument> Images { get; init; } = [];
    }
    public record ImageDocument
    {
        public required string Url { get; init; }
        public int Position { get; init; }
    }

}

using Meilisearch;
using Search.Service.Documents;
using Search.Service.Dtos;
using StackExchange.Redis;

namespace Search.Service.Workers;

public class VehicleIndexerService : BackgroundService
{
    private readonly ILogger<VehicleIndexerService> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly MeilisearchClient _meilisearch;
    private readonly HttpClient _httpClient;

    public VehicleIndexerService(ILogger<VehicleIndexerService> logger, IConnectionMultiplexer redis, MeilisearchClient meilisearch)
    {
        _logger = logger;
        _redis = redis;
        _meilisearch = meilisearch;
        _httpClient = new HttpClient { BaseAddress = new Uri("http://catalog.api:8080") };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Vehicle Indexer Service is running.");

        var subscriber = _redis.GetSubscriber();

        // Subscription for creating/updating vehicles
        await subscriber.SubscribeAsync("vehicles", async (channel, message) =>
        {
            if (Guid.TryParse(message, out var vehicleId))
            {
                _logger.LogInformation($"Indexing message received for vehicle: {vehicleId}");
                try
                {
                    // Use a strongly-typed DTO for safe data retrieval
                    var vehicleData = await _httpClient.GetFromJsonAsync<VehicleSearchResponse>($"/api/vehicles/{vehicleId}", stoppingToken);

                    if (vehicleData != null)
                    {
                        var document = new VehicleDocument
                        {
                            Id = vehicleData.Id,
                            Brand = vehicleData.Brand,
                            Model = vehicleData.Model,
                            Year = vehicleData.Year,
                            Price = vehicleData.Price,
                            Mileage = vehicleData.Mileage,
                            Status = vehicleData.Status,
                            CoverImageUrl = vehicleData.CoverImageUrl,
                            Images = vehicleData.Images.Select(img => new ImageDocument { Url = img.Url, Position = img.Position }).ToList(),
                            VehicleType = vehicleData.VehicleType,
                            Features = vehicleData.Features.Select(f => f.Name).ToList()
                        };

                        var index = _meilisearch.Index("vehicles");
                        await index.AddDocumentsAsync(new[] { document }, "id", stoppingToken);
                        _logger.LogInformation($"Vehicle {vehicleId} indexed successfully.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing vehicle {vehicleId}.");
                }
            }
        });

        // Subscription for deleting vehicles
        await subscriber.SubscribeAsync("vehicles_deleted", async (channel, message) =>
        {
            if (Guid.TryParse(message, out var vehicleId))
            {
                _logger.LogInformation($"Deletion message received for vehicle: {vehicleId}");
                try
                {
                    var index = _meilisearch.Index("vehicles");
                    await index.DeleteOneDocumentAsync(vehicleId.ToString(), stoppingToken);
                    _logger.LogInformation($"Vehicle {vehicleId} deleted from Meilisearch index.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deleting vehicle {vehicleId} from index.");
                }
            }
        });
    }
}
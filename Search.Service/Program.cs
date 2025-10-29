using Meilisearch;
using Search.Service.Workers;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// --- SERVICES ---
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("redis:6379, abortConnect=false"));

builder.Services.AddSingleton(provider => new MeilisearchClient(
    "http://meilisearch:7700", "mySecureMasterKey123456789ABC"
));

builder.Services.AddHostedService<VehicleIndexerService>();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// --- CONFIGURE MEILISEARCH ON STARTUP ---
try
{
    var client = app.Services.GetRequiredService<MeilisearchClient>();
    var index = client.Index("vehicles");

    // CORRECTION: Both searchable and filterable attributes MUST be in camelCase
    await index.UpdateSearchableAttributesAsync(new[] { "brand", "model", "features", "year" });
    await index.UpdateFilterableAttributesAsync(new[] { "brand", "vehicleType", "year", "price" });

    app.Logger.LogInformation("Meilisearch index configured successfully.");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to configure Meilisearch index.");
}
// --- END CONFIGURATION ---

// --- PIPELINE ---
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
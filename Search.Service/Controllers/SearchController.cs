using Meilisearch;
using Microsoft.AspNetCore.Mvc;
using Search.Service.Documents;
using Search.Service.Dtos;

namespace Search.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly MeilisearchClient _meilisearch;
    private readonly ILogger<SearchController> _logger;

    public SearchController(MeilisearchClient meilisearch, ILogger<SearchController> logger)
    {
        _meilisearch = meilisearch;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string query = "", [FromQuery] string? filter = null)
    {
        //CORRECCIÓN: Reemplaza comillas simples por dobles para Meilisearch
        var meilisearchFilter = filter?.Replace("'", "\"");

        _logger.LogInformation($"Búsqueda recibida: '{query}', Filtro Meilisearch: '{meilisearchFilter}'");

        try
        {
            var index = _meilisearch.Index("vehicles");
            var searchConfig = new SearchQuery
            {
                Filter = meilisearchFilter
            };

            var results = await index.SearchAsync<VehicleDocument>(query, searchConfig);

            var response = results.Hits.Select(doc => new VehicleSearchResponse
            {
                Id = doc.Id,
                Brand = doc.Brand,
                Model = doc.Model,
                Year = doc.Year,
                Price = doc.Price,
                Mileage = doc.Mileage,
                Status = doc.Status,
                CoverImageUrl = doc.CoverImageUrl,
                Images = doc.Images.Select(img => new ImageSearchResponse { Url = img.Url, Position = img.Position }).ToList(),
                VehicleType = doc.VehicleType,
                Features = doc.Features.Select(fName => new FeatureSearchResponse { Name = fName }).ToList()
            });

            _logger.LogInformation($"Búsqueda completada, {response.Count()} resultados encontrados.");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la búsqueda");
            return StatusCode(500, "Ocurrió un error en el servicio de búsqueda.");
        }
    }
}
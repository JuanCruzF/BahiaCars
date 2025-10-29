using Catalog.Api.Dtos;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeaturesController : ControllerBase
{
    private readonly CatalogDbContext _context;

    public FeaturesController(CatalogDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _context.Features.ToListAsync());
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] FeatureDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("El nombre del equipamiento no puede estar vacío.");
        }

        var feature = new Feature(dto.Name, dto.Category);
        _context.Features.Add(feature);
        await _context.SaveChangesAsync();
        return Ok(feature);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var feature = await _context.Features.FindAsync(id);

        if (feature == null)
        {
            return NotFound();
        }

        _context.Features.Remove(feature);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using FFMpegCore;
using FFMpegCore.Enums;

namespace Media.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MediaController> _logger;

    public MediaController(IWebHostEnvironment env, ILogger<MediaController> logger)
    {
        _env = env;
        _logger = logger;
    }

    // POST api/media/upload
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No se ha enviado ningún archivo.");
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var uploadPath = Path.Combine(_env.ContentRootPath, "uploads");

        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var filePath = Path.Combine(uploadPath, fileName);

        _logger.LogInformation($"Guardando archivo en: {filePath}");

        if (file.ContentType.StartsWith("image/"))
        {
            using var image = await Image.LoadAsync(file.OpenReadStream());
            image.Mutate(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(1280, 0) }));
            await image.SaveAsJpegAsync(filePath, new JpegEncoder { Quality = 80 });
        }
        else if (file.ContentType.StartsWith("video/"))
        {
            var tempPath = Path.GetTempFileName() + Path.GetExtension(file.FileName);
            await using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            await FFMpegArguments
                .FromFileInput(tempPath)
                .OutputToFile(filePath, false, options => options
                    .WithVideoFilters(filterOptions => filterOptions.Scale(width: -1, height: 720))
                    .WithConstantRateFactor(24))
                .ProcessAsynchronously();
            System.IO.File.Delete(tempPath);
        }
        else
        {
            return BadRequest("Formato de archivo no soportado.");
        }

        // Devolver URL relativa
        var publicUrl = $"/uploads/{fileName}";
        _logger.LogInformation($"Archivo guardado exitosamente. URL: {publicUrl}");

        var response = new UploadResponseDto(publicUrl, file.ContentType, file.Length);
        return Ok(response);
    }

    // GET api/media/uploads/{fileName}
    [HttpGet("uploads/{fileName}")]
    public IActionResult GetFile(string fileName)
    {
        var filePath = Path.Combine(_env.ContentRootPath, "uploads", fileName);

        _logger.LogInformation($"Buscando archivo: {filePath}");

        if (!System.IO.File.Exists(filePath))
        {
            _logger.LogWarning($"Archivo no encontrado: {filePath}");
            return NotFound(new { error = $"Archivo no encontrado: {fileName}" });
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            _ => "application/octet-stream"
        };

        _logger.LogInformation($"Sirviendo archivo {fileName} con content-type {contentType}");
        return PhysicalFile(filePath, contentType);
    }
    // DELETE: api/media/uploads/nombre-del-archivo.jpg
    [HttpDelete("uploads/{fileName}")]
    public IActionResult DeleteFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest("El nombre del archivo no puede estar vacío.");
        }

        // Medida de seguridad para evitar que se borren archivos fuera de la carpeta 'uploads'
        var uploadPath = Path.Combine(_env.ContentRootPath, "uploads");
        var fullPath = Path.Combine(uploadPath, fileName);

        // Normaliza las rutas para la comparación de seguridad
        var normalizedUploadPath = Path.GetFullPath(uploadPath);
        var normalizedFullPath = Path.GetFullPath(fullPath);

        if (!normalizedFullPath.StartsWith(normalizedUploadPath))
        {
            return Forbid("Intento de acceso a una ruta no permitida.");
        }

        try
        {
            if (System.IO.File.Exists(normalizedFullPath))
            {
                System.IO.File.Delete(normalizedFullPath);
                return NoContent(); // Éxito
            }
            else
            {
                return NotFound(); // El archivo no existe
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno al borrar el archivo: {ex.Message}");
        }
    }
}

public record UploadResponseDto(string Url, string ContentType, long OriginalSize);
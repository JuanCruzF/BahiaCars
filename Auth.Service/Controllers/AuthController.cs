using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("login")]
    [AllowAnonymous] // <-- Esta línea es la clave, hace este método 100% público
    public IActionResult Login([FromBody] LoginDto login)
    {
        if ((login.Username == "admin" && login.Password == "password123"))
        {
            var token = GenerateJwtToken(login.Username);
            return Ok(new { token });
        }

        return Unauthorized(new { message = "Usuario o contraseña inválidos" });
    }

    // Si tuvieras otros métodos aquí que sí requieren token, les pondrías [Authorize]
    // Ejemplo:
    // [HttpGet("validate")]
    // [Authorize]
    // public IActionResult ValidateToken()
    // {
    //     return Ok(new { user = User.Identity?.Name });
    // }

    private string GenerateJwtToken(string username)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8), // Usar UtcNow es mejor práctica
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginDto(string Username, string Password);
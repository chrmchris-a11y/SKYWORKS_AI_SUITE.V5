using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    public record LoginRequest(string Username, string Password);

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        // Very simple dev-only validation (Phase1): users in appsettings
        var usersSection = _config.GetSection("Users");
        var userPass = usersSection[req.Username];
        // Fallback to defaults if config missing (Phase1 convenience)
        if (string.IsNullOrEmpty(userPass) && req.Username == "admin") userPass = "admin123";
        if (string.IsNullOrEmpty(userPass) || userPass != req.Password)
        {
            return Unauthorized(new { error = "Invalid credentials" });
        }

        var issuer = _config["Jwt:Issuer"] ?? "Skyworks";
        var audience = _config["Jwt:Audience"] ?? "SkyworksUsers";
        var keyStr = _config["Jwt:Key"] ?? "dev_secret_key_change_me";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, req.Username),
            new Claim(ClaimTypes.Name, req.Username),
            new Claim(ClaimTypes.Role, "User")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new { access_token = jwt, token_type = "Bearer", expires_in = 8 * 3600 });
    }
}

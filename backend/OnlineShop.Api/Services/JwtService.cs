using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user)
    {
        // -----------------------------
        // Validate configuration values
        // -----------------------------
        var keyString = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is missing in configuration.");

        var issuer = _config["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is missing in configuration.");

        var audience = _config["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is missing in configuration.");

        var expiresString = _config["Jwt:ExpiresInMinutes"]
            ?? throw new InvalidOperationException("Jwt:ExpiresInMinutes is missing in configuration.");

        if (!int.TryParse(expiresString, out var expiresInMinutes))
            throw new InvalidOperationException("Jwt:ExpiresInMinutes must be a valid integer.");

        // -----------------------------
        // Build signing key
        // -----------------------------
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // -----------------------------
        // Claims
        // -----------------------------
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

        foreach (var role in user.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        // -----------------------------
        // Token creation
        // -----------------------------
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetExpiration()
    {
        var expiresString = _config["Jwt:ExpiresInMinutes"]
            ?? throw new InvalidOperationException("Jwt:ExpiresInMinutes is missing in configuration.");

        if (!int.TryParse(expiresString, out var expiresInMinutes))
            throw new InvalidOperationException("Jwt:ExpiresInMinutes must be a valid integer.");

        return DateTime.UtcNow.AddMinutes(expiresInMinutes);
    }
}

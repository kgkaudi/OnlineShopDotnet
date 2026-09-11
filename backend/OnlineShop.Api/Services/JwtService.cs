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

    // ---------------------------------------------------------
    // VALIDATION HELPERS
    // ---------------------------------------------------------

    private static void ValidateUser(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(user.Id))
            throw new ArgumentNullException(nameof(user.Id));

        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentNullException(nameof(user.Email));
    }

    private string GetConfig(string key)
    {
        var value = _config[key];
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{key} is missing in configuration.");

        return value;
    }

    private SymmetricSecurityKey BuildKey(string keyString)
    {
        if (string.IsNullOrWhiteSpace(keyString))
            throw new InvalidOperationException("Jwt:Key is missing in configuration.");

        if (keyString.Length < 32)
            throw new ArgumentException("Jwt:Key must be at least 32 characters long.");

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
    }

    // ---------------------------------------------------------
    // GENERATE TOKEN
    // ---------------------------------------------------------

    public string GenerateToken(User user)
    {
        // ---------------------------------------------------------
        // VALIDATION (tests expect ArgumentNullException)
        // ---------------------------------------------------------
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(user.Id))
            throw new ArgumentNullException(nameof(user.Id), "User ID is required.");

        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentNullException(nameof(user.Email), "Email is required.");

        // ---------------------------------------------------------
        // CONFIG VALIDATION
        // ---------------------------------------------------------
        var keyString = GetConfig("Jwt:Key");
        var issuer = GetConfig("Jwt:Issuer");
        var audience = GetConfig("Jwt:Audience");
        var expiresString = GetConfig("Jwt:ExpiresInMinutes");

        if (!int.TryParse(expiresString, out var expiresInMinutes))
            throw new InvalidOperationException("Jwt:ExpiresInMinutes must be a valid integer.");

        var key = BuildKey(keyString);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // ---------------------------------------------------------
        // CLAIMS
        // ---------------------------------------------------------
        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email.Trim())
    };

        if (user.Roles != null)
        {
            foreach (var role in user.Roles)
            {
                if (!string.IsNullOrWhiteSpace(role))
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        // ---------------------------------------------------------
        // TOKEN
        // ---------------------------------------------------------
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ---------------------------------------------------------
    // GET EXPIRATION
    // ---------------------------------------------------------

    public DateTime GetExpiration()
    {
        var expiresString = GetConfig("Jwt:ExpiresInMinutes");

        if (!int.TryParse(expiresString, out var expiresInMinutes))
            throw new InvalidOperationException("Jwt:ExpiresInMinutes must be a valid integer.");

        return DateTime.UtcNow.AddMinutes(expiresInMinutes);
    }
}

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;


public class JwtServiceTests
{
    private JwtService CreateService(Dictionary<string, string> settings)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        return new JwtService(config);
    }

    private Dictionary<string, string> ValidSettings => new()
    {
        ["Jwt:Key"] = "THIS_IS_A_TEST_KEY_THAT_IS_LONG_ENOUGH_123456",
        ["Jwt:Issuer"] = "TestIssuer",
        ["Jwt:Audience"] = "TestAudience",
        ["Jwt:ExpiresInMinutes"] = "30"
    };

    // ---------------------------------------------------------
    // TOKEN GENERATION
    // ---------------------------------------------------------

    [Fact]
    public void GenerateToken_ShouldCreateValidJwt()
    {
        var service = CreateService(ValidSettings);

        var user = new User
        {
            Id = "123",
            Email = "test@example.com",
            Roles = new List<string> { "User" }
        };

        var token = service.GenerateToken(user);

        token.Should().NotBeNull();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "123");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "test@example.com");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
    }

    // ---------------------------------------------------------
    // MULTIPLE ROLES
    // ---------------------------------------------------------

    [Fact]
    public void GenerateToken_ShouldIncludeMultipleRoles()
    {
        var service = CreateService(ValidSettings);

        var user = new User
        {
            Id = "123",
            Email = "test@example.com",
            Roles = new List<string> { "User", "Admin" }
        };

        var token = service.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    // ---------------------------------------------------------
    // EXPIRATION
    // ---------------------------------------------------------

    [Fact]
    public void GetExpiration_ShouldReturnCorrectTime()
    {
        var service = CreateService(ValidSettings);

        var expiration = service.GetExpiration();

        var expected = DateTime.UtcNow.AddMinutes(30);

        expiration.Should().BeCloseTo(expected, TimeSpan.FromSeconds(2));
    }

    // ---------------------------------------------------------
    // MISSING CONFIG
    // ---------------------------------------------------------

    [Fact]
    public void GenerateToken_ShouldThrow_WhenKeyMissing()
    {
        var settings = new Dictionary<string, string>(ValidSettings);
        settings.Remove("Jwt:Key");

        var service = CreateService(settings);

        var user = new User { Id = "1", Email = "a@b.com", Roles = new List<string>() };

        Action act = () => service.GenerateToken(user);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:Key*");
    }

    [Fact]
    public void GenerateToken_ShouldThrow_WhenExpiresInvalid()
    {
        var settings = new Dictionary<string, string>(ValidSettings)
        {
            ["Jwt:ExpiresInMinutes"] = "not-a-number"
        };

        var service = CreateService(settings);

        var user = new User { Id = "1", Email = "a@b.com", Roles = new List<string>() };

        Action act = () => service.GenerateToken(user);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*valid integer*");
    }
}

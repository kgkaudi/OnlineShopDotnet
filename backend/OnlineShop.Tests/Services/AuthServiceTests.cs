using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using OnlineShop.Api.Services;
using Xunit;

public class AuthServiceTests : RepositoryTestBase
{
    private readonly AuthService _auth;
    private readonly UserRepository _userRepo;
    private readonly JwtService _jwt;

    public AuthServiceTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var dbConfig = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _userRepo = new UserRepository(dbConfig);

        var jwtConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "THIS_IS_A_LONG_ENOUGH_TEST_KEY_1234567890",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiresInMinutes"] = "30"
            })
            .Build();

        _jwt = new JwtService(jwtConfig);

        // FIX: AuthService now requires (config, jwt, repo)
        _auth = new AuthService(jwtConfig, _jwt, _userRepo);

        Fixture.Database.DropCollection("Users");
    }

    // ---------------------------------------------------------
    // REGISTER
    // ---------------------------------------------------------

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenEmailIsNew()
    {
        var user = await _auth.RegisterAsync("test@example.com", "password123", "Test User");

        user.Should().NotBeNull();
        user!.Email.Should().Be("test@example.com");

        var fetched = await _userRepo.GetByEmailAsync("test@example.com");
        fetched.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenEmailExists()
    {
        await _auth.RegisterAsync("duplicate@example.com", "pass", "User1");

        var result = await _auth.RegisterAsync("duplicate@example.com", "pass", "User2");
        result.Should().BeNull();
    }

    // ---------------------------------------------------------
    // LOGIN
    // ---------------------------------------------------------

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreCorrect()
    {
        await _auth.RegisterAsync("login@example.com", "mypassword", "Login User");

        var token = await _auth.LoginAsync("login@example.com", "mypassword");

        token.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsWrong()
    {
        await _auth.RegisterAsync("wrongpass@example.com", "correct", "User");

        var token = await _auth.LoginAsync("wrongpass@example.com", "incorrect");
        token.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var token = await _auth.LoginAsync("missing@example.com", "pass");
        token.Should().BeNull();
    }

    // ---------------------------------------------------------
    // ADD ROLE
    // ---------------------------------------------------------

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserIdIsInvalid()
    {
        var result = await _auth.AddRoleAsync("invalid-id", "Admin");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _auth.AddRoleAsync(id, "Admin");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldAddRole_WhenUserExists()
    {
        var user = new User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = "role@example.com",
            FullName = "Role User",
            PasswordHash = "HASH",
            Roles = new List<string> { "User" }
        };

        await _userRepo.CreateAsync(user);

        var updated = await _auth.AddRoleAsync(user.Id, "Admin");
        updated.Should().BeTrue();

        var fetched = await _userRepo.GetByIdAsync(user.Id);
        fetched!.Roles.Should().Contain("Admin");
    }
}

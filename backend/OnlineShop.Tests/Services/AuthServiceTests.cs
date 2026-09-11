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

        _auth = new AuthService(jwtConfig, _jwt, _userRepo);

        Fixture.Database.DropCollection("Users");
    }

    // ---------------------------------------------------------
    // REGISTER — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenEmailIsNull()
    {
        var result = await _auth.RegisterAsync(null!, "pass", "User");
        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenEmailIsWhitespace()
    {
        var result = await _auth.RegisterAsync("   ", "pass", "User");
        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenPasswordIsNull()
    {
        var result = await _auth.RegisterAsync("test@test.com", null!, "User");
        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenPasswordIsWhitespace()
    {
        var result = await _auth.RegisterAsync("test@test.com", "   ", "User");
        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenFullNameIsNull()
    {
        var result = await _auth.RegisterAsync("test@test.com", "pass", null!);
        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenFullNameIsWhitespace()
    {
        var result = await _auth.RegisterAsync("test@test.com", "pass", "   ");
        result.Should().BeNull();
    }

    // ---------------------------------------------------------
    // REGISTER — EXISTING EMAIL
    // ---------------------------------------------------------

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenEmailAlreadyExists_CaseInsensitive()
    {
        await _auth.RegisterAsync("duplicate@test.com", "pass", "User1");

        var result = await _auth.RegisterAsync("DUPLICATE@test.com", "pass", "User2");
        result.Should().BeNull();
    }

    // ---------------------------------------------------------
    // LOGIN — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenEmailIsNull()
    {
        var result = await _auth.LoginAsync(null!, "pass");
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenEmailIsWhitespace()
    {
        var result = await _auth.LoginAsync("   ", "pass");
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsNull()
    {
        var result = await _auth.LoginAsync("test@test.com", null!);
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsWhitespace()
    {
        var result = await _auth.LoginAsync("test@test.com", "   ");
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsEmpty()
    {
        var result = await _auth.LoginAsync("test@test.com", "");
        result.Should().BeNull();
    }

    // ---------------------------------------------------------
    // LOGIN — SECURITY EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsCorrectButEmailCaseDiffers()
    {
        await _auth.RegisterAsync("case@test.com", "pass123", "User");

        var result = await _auth.LoginAsync("CASE@test.com", "pass123");
        result.Should().BeNull(); // email match must be exact
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsCorrectButUserIsDeleted()
    {
        var user = await _auth.RegisterAsync("delete@test.com", "pass123", "User");

        await _userRepo.DeleteAsync(user!.Id);

        var result = await _auth.LoginAsync("delete@test.com", "pass123");
        result.Should().BeNull();
    }

    // ---------------------------------------------------------
    // ADD ROLE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserIdIsNull()
    {
        var result = await _auth.AddRoleAsync(null!, "Admin");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserIdIsWhitespace()
    {
        var result = await _auth.AddRoleAsync("   ", "Admin");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenRoleIsNull()
    {
        var user = await _auth.RegisterAsync("role@test.com", "pass", "User");
        var result = await _auth.AddRoleAsync(user!.Id, null!);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenRoleIsWhitespace()
    {
        var user = await _auth.RegisterAsync("role2@test.com", "pass", "User");
        var result = await _auth.AddRoleAsync(user!.Id, "   ");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserIdIsInvalidObjectId()
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
    public async Task AddRoleAsync_ShouldNotDuplicateRole_WhenRoleAlreadyExists()
    {
        var user = await _auth.RegisterAsync("dup@test.com", "pass", "User");

        await _auth.AddRoleAsync(user!.Id, "Admin");
        var result = await _auth.AddRoleAsync(user.Id, "Admin");

        result.Should().BeTrue();

        var fetched = await _userRepo.GetByIdAsync(user.Id);
        fetched!.Roles.Should().HaveCount(1);
    }
}

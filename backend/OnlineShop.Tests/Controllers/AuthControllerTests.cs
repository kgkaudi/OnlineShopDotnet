using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Controllers;
using OnlineShop.Api.Dtos;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using Microsoft.Extensions.Configuration;
using OnlineShop.Api.Services;
using Xunit;
using MongoDB.Bson;

public class AuthControllerTests : RepositoryTestBase
{
    private readonly AuthController _controller;
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;
    private readonly UserRepository _userRepo;

    public AuthControllerTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var dbConfig = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _userRepo = new UserRepository(dbConfig);

        // JWT configuration for tests
        var jwtConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "THIS_IS_A_LONG_ENOUGH_TEST_KEY_1234567890",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiresInMinutes"] = "30"
            })
            .Build();

        _jwtService = new JwtService(jwtConfig);

        // AuthService now requires (IConfiguration, IJwtService, IUserRepository)
        _authService = new AuthService(jwtConfig, _jwtService, _userRepo);

        _controller = new AuthController(_authService, _jwtService);

        Fixture.Database.DropCollection("Users");
    }

    // ---------------------------------------------------------
    // REGISTER
    // ---------------------------------------------------------

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailMissing()
    {
        var dto = new RegisterDto
        {
            Email = "",
            Password = "pass123",
            FullName = "Test User"
        };

        var result = await _controller.Register(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenPasswordMissing()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "",
            FullName = "Test User"
        };

        var result = await _controller.Register(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenFullNameMissing()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "pass123",
            FullName = ""
        };

        var result = await _controller.Register(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailInvalid()
    {
        var dto = new RegisterDto
        {
            Email = "invalid-email",
            Password = "pass123",
            FullName = "Test User"
        };

        var result = await _controller.Register(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        var existing = new User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = "duplicate@example.com",
            FullName = "Existing User",
            PasswordHash = "hash",
            Roles = new List<string>()
        };

        await _userRepo.CreateAsync(existing);

        var dto = new RegisterDto
        {
            Email = "duplicate@example.com",
            Password = "pass123",
            FullName = "New User"
        };

        var result = await _controller.Register(dto);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenValid()
    {
        var dto = new RegisterDto
        {
            Email = "new@example.com",
            Password = "pass123",
            FullName = "New User"
        };

        var result = await _controller.Register(dto);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // LOGIN
    // ---------------------------------------------------------

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenEmailMissing()
    {
        var dto = new LoginDto
        {
            Email = "",
            Password = "pass123"
        };

        var result = await _controller.Login(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenPasswordMissing()
    {
        var dto = new LoginDto
        {
            Email = "test@example.com",
            Password = ""
        };

        var result = await _controller.Login(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenEmailInvalid()
    {
        var dto = new LoginDto
        {
            Email = "invalid-email",
            Password = "pass123"
        };

        var result = await _controller.Login(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsInvalid()
    {
        var dto = new LoginDto
        {
            Email = "wrong@example.com",
            Password = "wrongpass"
        };

        var result = await _controller.Login(dto);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenValid()
    {
        // Register user first
        var registerDto = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "pass123",
            FullName = "Valid User"
        };

        await _controller.Register(registerDto);

        // Login
        var loginDto = new LoginDto
        {
            Email = "valid@example.com",
            Password = "pass123"
        };

        var result = await _controller.Login(loginDto);

        result.Should().BeOfType<OkObjectResult>();
    }
}

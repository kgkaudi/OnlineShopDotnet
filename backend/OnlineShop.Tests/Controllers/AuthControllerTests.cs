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
        _authService = new AuthService(jwtConfig, _jwtService, _userRepo);
        _controller = new AuthController(_authService, _jwtService);

        Fixture.Database.DropCollection("Users");
    }

    // ---------------------------------------------------------
    // REGISTER
    // ---------------------------------------------------------

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenDtoNull()
    {
        var result = await _controller.Register(null!);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

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
    public async Task Register_ShouldReturnBadRequest_WhenEmailWhitespace()
    {
        var dto = new RegisterDto
        {
            Email = "   ",
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
    public async Task Register_ShouldReturnBadRequest_WhenPasswordTooShort()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "12",
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

    [Fact]
    public async Task Register_ShouldReturnOk_WhenEmailHasSpacesAround()
    {
        var dto = new RegisterDto
        {
            Email = "   spaced@example.com   ",
            Password = "pass123",
            FullName = "User"
        };

        var result = await _controller.Register(dto);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRegisteringTwiceDifferentEmails()
    {
        var dto1 = new RegisterDto
        {
            Email = "first@example.com",
            Password = "pass123",
            FullName = "User1"
        };

        var dto2 = new RegisterDto
        {
            Email = "second@example.com",
            Password = "pass123",
            FullName = "User2"
        };

        var r1 = await _controller.Register(dto1);
        var r2 = await _controller.Register(dto2);

        r1.Should().BeOfType<OkObjectResult>();
        r2.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // LOGIN
    // ---------------------------------------------------------

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenDtoNull()
    {
        var result = await _controller.Login(null!);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

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
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        var dto = new LoginDto
        {
            Email = "notfound@example.com",
            Password = "pass123"
        };

        var result = await _controller.Login(dto);
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenWrongPassword()
    {
        var registerDto = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "pass123",
            FullName = "Valid User"
        };

        await _controller.Register(registerDto);

        var loginDto = new LoginDto
        {
            Email = "valid@example.com",
            Password = "wrongpass"
        };

        var result = await _controller.Login(loginDto);
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenValid()
    {
        var registerDto = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "pass123",
            FullName = "Valid User"
        };

        await _controller.Register(registerDto);

        var loginDto = new LoginDto
        {
            Email = "valid@example.com",
            Password = "pass123"
        };

        var result = await _controller.Login(loginDto);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenEmailHasSpacesAround()
    {
        var registerDto = new RegisterDto
        {
            Email = "trim@example.com",
            Password = "pass123",
            FullName = "Trim User"
        };

        await _controller.Register(registerDto);

        var loginDto = new LoginDto
        {
            Email = "   trim@example.com   ",
            Password = "pass123"
        };

        var result = await _controller.Login(loginDto);
        result.Should().BeOfType<OkObjectResult>();
    }
}

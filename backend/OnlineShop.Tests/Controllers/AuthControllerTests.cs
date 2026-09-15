using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using OnlineShop.Api.Controllers;
using OnlineShop.Api.Dtos;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using OnlineShop.Api.Services;
using Xunit;

public class AuthControllerTests : RepositoryTestBase
{
    private readonly AuthController _controller;
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;
    private readonly UserRepository _userRepo;
    private readonly InvalidTokenRepository _invalidTokens;

    public AuthControllerTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var dbConfig = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _userRepo = new UserRepository(dbConfig);
        _invalidTokens = new InvalidTokenRepository(dbConfig);

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
        _authService = new AuthService(
            jwtConfig,
            _jwtService,
            _userRepo,
            _invalidTokens);

        _controller = new AuthController(
            _authService,
            _jwtService);

        Fixture.Database.DropCollection("Users");
        Fixture.Database.DropCollection("InvalidTokens");
    }

    // =========================================================
    // REGISTER - VALIDATION
    // =========================================================

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
    public async Task Register_ShouldReturnBadRequest_WhenEmailNull()
    {
        var dto = new RegisterDto
        {
            Email = null!,
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
    public async Task Register_ShouldReturnBadRequest_WhenPasswordWhitespace()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "   ",
            FullName = "Test User"
        };

        var result = await _controller.Register(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenPasswordNull()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = null!,
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
    public async Task Register_ShouldReturnBadRequest_WhenFullNameWhitespace()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "pass123",
            FullName = "   "
        };

        var result = await _controller.Register(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenFullNameNull()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "pass123",
            FullName = null!
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

    [Theory]
    [InlineData("invalid")]
    [InlineData("test")]
    [InlineData("test.com")]
    public async Task Register_ShouldReturnBadRequest_WhenEmailDoesNotContainAtSymbol(
    string email)
    {
        var dto = new RegisterDto
        {
            Email = email,
            Password = "pass123",
            FullName = "Test User"
        };

        var result = await _controller.Register(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("12")]
    [InlineData("  ")]
    public async Task Register_ShouldReturnBadRequest_WhenPasswordLengthTooShort(
        string password)
    {
        var dto = new RegisterDto
        {
            Email = $"short-{Guid.NewGuid()}@example.com",
            Password = password,
            FullName = "Test User"
        };

        var result = await _controller.Register(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // =========================================================
    // REGISTER - SUCCESS
    // =========================================================

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
    public async Task Register_ShouldTrimEmail()
    {
        var dto = new RegisterDto
        {
            Email = "   spaced@example.com   ",
            Password = "pass123",
            FullName = "User"
        };

        var result = await _controller.Register(dto);

        result.Should().BeOfType<OkObjectResult>();

        var user = await _userRepo.GetByEmailAsync("spaced@example.com");

        user.Should().NotBeNull();
        user!.Email.Should().Be("spaced@example.com");
    }

    [Fact]
    public async Task Register_ShouldNormalizeEmailToLowercase()
    {
        var dto = new RegisterDto
        {
            Email = "UPPERCASE@EXAMPLE.COM",
            Password = "pass123",
            FullName = "User"
        };

        var result = await _controller.Register(dto);

        result.Should().BeOfType<OkObjectResult>();

        var user = await _userRepo.GetByEmailAsync("uppercase@example.com");

        user.Should().NotBeNull();
        user!.Email.Should().Be("uppercase@example.com");
    }

    [Fact]
    public async Task Register_ShouldTrimFullName()
    {
        var dto = new RegisterDto
        {
            Email = "fullname@example.com",
            Password = "pass123",
            FullName = "   Test User   "
        };

        var result = await _controller.Register(dto);

        result.Should().BeOfType<OkObjectResult>();

        var user = await _userRepo.GetByEmailAsync("fullname@example.com");

        user.Should().NotBeNull();
        user!.FullName.Should().Be("Test User");
    }

    [Fact]
    public async Task Register_ShouldPersistPasswordAsHash()
    {
        var dto = new RegisterDto
        {
            Email = "hash@example.com",
            Password = "pass123",
            FullName = "Hash User"
        };

        await _controller.Register(dto);

        var user = await _userRepo.GetByEmailAsync("hash@example.com");

        user.Should().NotBeNull();
        user!.PasswordHash.Should().NotBe("pass123");
        user.PasswordHash.Should().NotBeNullOrWhiteSpace();

        BCrypt.Net.BCrypt.Verify(
            "pass123",
            user.PasswordHash)
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task Register_ShouldCreateUserWithDefaultUserRole()
    {
        var dto = new RegisterDto
        {
            Email = "role@example.com",
            Password = "pass123",
            FullName = "Role User"
        };

        await _controller.Register(dto);

        var user = await _userRepo.GetByEmailAsync("role@example.com");

        user.Should().NotBeNull();
        user!.Roles.Should().ContainSingle();
        user.Roles.Should().Contain("User");
    }

    [Fact]
    public async Task Register_ShouldSetCreatedAt()
    {
        var before = DateTime.UtcNow;

        var dto = new RegisterDto
        {
            Email = "created@example.com",
            Password = "pass123",
            FullName = "Created User"
        };

        await _controller.Register(dto);

        var after = DateTime.UtcNow;

        var user = await _userRepo.GetByEmailAsync("created@example.com");

        user.Should().NotBeNull();
        user!.CreatedAt.Should().BeOnOrAfter(before.AddSeconds(-1));
        user.CreatedAt.Should().BeOnOrBefore(after.AddSeconds(1));
    }

    [Fact]
    public async Task Register_ShouldSetUpdatedAt()
    {
        var dto = new RegisterDto
        {
            Email = "updated@example.com",
            Password = "pass123",
            FullName = "Updated User"
        };

        await _controller.Register(dto);

        var user = await _userRepo.GetByEmailAsync("updated@example.com");

        user.Should().NotBeNull();
        user!.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task Register_ShouldSetEmailAsUnverified()
    {
        var dto = new RegisterDto
        {
            Email = "verification@example.com",
            Password = "pass123",
            FullName = "Verification User"
        };

        await _controller.Register(dto);

        var user = await _userRepo.GetByEmailAsync(
            "verification@example.com");

        user.Should().NotBeNull();
        user!.IsEmailVerified.Should().BeFalse();
    }

    // =========================================================
    // REGISTER - PROFILE FIELDS
    // =========================================================

    [Fact]
    public async Task Register_ShouldPersistPhoneNumber()
    {
        var dto = new RegisterDto
        {
            Email = "phone@example.com",
            Password = "pass123",
            FullName = "Phone User",
            PhoneNumber = "123456789"
        };

        await _controller.Register(dto);

        var user = await _userRepo.GetByEmailAsync("phone@example.com");

        user.Should().NotBeNull();
        user!.PhoneNumber.Should().Be("123456789");
    }

    [Fact]
    public async Task Register_ShouldTrimPhoneNumber()
    {
        var dto = new RegisterDto
        {
            Email = "phone-trim@example.com",
            Password = "pass123",
            FullName = "Phone User",
            PhoneNumber = "   123456789   "
        };

        await _controller.Register(dto);

        var user = await _userRepo.GetByEmailAsync(
            "phone-trim@example.com");

        user.Should().NotBeNull();
        user!.PhoneNumber.Should().Be("123456789");
    }

    [Fact]
    public async Task Register_ShouldStoreNullPhoneNumber_WhenWhitespace()
    {
        var dto = new RegisterDto
        {
            Email = "phone-null@example.com",
            Password = "pass123",
            FullName = "Phone User",
            PhoneNumber = "   "
        };

        await _controller.Register(dto);

        var user = await _userRepo.GetByEmailAsync(
            "phone-null@example.com");

        user.Should().NotBeNull();
        user!.PhoneNumber.Should().BeNull();
    }

    [Fact]
    public async Task Register_ShouldPersistShippingAddress()
    {
        var dto = new RegisterDto
        {
            Email = "shipping@example.com",
            Password = "pass123",
            FullName = "Shipping User",
            ShippingAddress = new Address
            {
                Street = "Main Street 1",
                City = "Gothenburg",
                State = "Vastra Gotaland",
                PostalCode = "41101",
                Country = "Sweden"
            }
        };

        await _controller.Register(dto);

        var user = await _userRepo.GetByEmailAsync(
            "shipping@example.com");

        user.Should().NotBeNull();
        user!.ShippingAddress.Should().NotBeNull();

        user.ShippingAddress!.Street.Should().Be("Main Street 1");
        user.ShippingAddress.City.Should().Be("Gothenburg");
        user.ShippingAddress.PostalCode.Should().Be("41101");
        user.ShippingAddress.Country.Should().Be("Sweden");
    }

    [Fact]
    public async Task Register_ShouldPersistBillingAddress()
    {
        var dto = new RegisterDto
        {
            Email = "billing@example.com",
            Password = "pass123",
            FullName = "Billing User",
            BillingAddress = new Address
            {
                Street = "Billing Street 5",
                City = "Stockholm",
                State = "Stockholm",
                PostalCode = "11120",
                Country = "Sweden"
            }
        };

        await _controller.Register(dto);

        var user = await _userRepo.GetByEmailAsync(
            "billing@example.com");

        user.Should().NotBeNull();
        user!.BillingAddress.Should().NotBeNull();

        user.BillingAddress!.Street.Should().Be("Billing Street 5");
        user.BillingAddress.City.Should().Be("Stockholm");
        user.BillingAddress.PostalCode.Should().Be("11120");
        user.BillingAddress.Country.Should().Be("Sweden");
    }

    [Fact]
    public async Task Register_ShouldPersistCompleteProfile()
    {
        var dto = new RegisterDto
        {
            Email = "complete@example.com",
            Password = "pass123",
            FullName = "Complete User",
            PhoneNumber = "0701234567",

            ShippingAddress = new Address
            {
                Street = "Shipping 1",
                City = "Gothenburg",
                State = "Vastra Gotaland",
                PostalCode = "41101",
                Country = "Sweden"
            },

            BillingAddress = new Address
            {
                Street = "Billing 2",
                City = "Stockholm",
                State = "Stockholm",
                PostalCode = "11120",
                Country = "Sweden"
            }
        };

        await _controller.Register(dto);

        var user = await _userRepo.GetByEmailAsync(
            "complete@example.com");

        user.Should().NotBeNull();

        user!.FullName.Should().Be("Complete User");
        user.PhoneNumber.Should().Be("0701234567");

        user.ShippingAddress.Should().NotBeNull();
        user.BillingAddress.Should().NotBeNull();

        user.ShippingAddress!.City.Should().Be("Gothenburg");
        user.BillingAddress!.City.Should().Be("Stockholm");
    }

    [Fact]
    public async Task Register_ShouldAllowNullOptionalProfileFields()
    {
        var dto = new RegisterDto
        {
            Email = "optional@example.com",
            Password = "pass123",
            FullName = "Optional User",
            PhoneNumber = null,
            ShippingAddress = null,
            BillingAddress = null
        };

        var result = await _controller.Register(dto);

        result.Should().BeOfType<OkObjectResult>();

        var user = await _userRepo.GetByEmailAsync(
            "optional@example.com");

        user.Should().NotBeNull();
        user!.PhoneNumber.Should().BeNull();
        user.ShippingAddress.Should().BeNull();
        user.BillingAddress.Should().BeNull();
    }

    // =========================================================
    // REGISTER - DUPLICATES
    // =========================================================

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        var existing = new User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = "duplicate@example.com",
            FullName = "Existing User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123"),
            Roles = new List<string> { "User" }
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
    public async Task Register_ShouldReturnConflict_WhenDuplicateEmailDiffersOnlyByCase()
    {
        var first = new RegisterDto
        {
            Email = "duplicate@example.com",
            Password = "pass123",
            FullName = "First User"
        };

        var second = new RegisterDto
        {
            Email = "DUPLICATE@EXAMPLE.COM",
            Password = "pass123",
            FullName = "Second User"
        };

        var firstResult = await _controller.Register(first);
        var secondResult = await _controller.Register(second);

        firstResult.Should().BeOfType<OkObjectResult>();
        secondResult.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenDuplicateEmailHasWhitespace()
    {
        var first = new RegisterDto
        {
            Email = "duplicate-space@example.com",
            Password = "pass123",
            FullName = "First User"
        };

        await _controller.Register(first);

        var second = new RegisterDto
        {
            Email = "   duplicate-space@example.com   ",
            Password = "pass123",
            FullName = "Second User"
        };

        var result = await _controller.Register(second);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Register_ShouldAllowDifferentEmails()
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

        var users = await _userRepo.GetAllAsync();

        users.Should().HaveCount(2);
    }

    // =========================================================
    // REGISTER - RESPONSE
    // =========================================================

    [Fact]
    public async Task Register_Response_ShouldContainUserId()
    {
        var dto = new RegisterDto
        {
            Email = "response-id@example.com",
            Password = "pass123",
            FullName = "Response User"
        };

        var result = await _controller.Register(dto);

        var ok = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        ok.Value.Should().NotBeNull();

        var id = ok.Value!
            .GetType()
            .GetProperty("id")!
            .GetValue(ok.Value)
            ?.ToString();

        id.Should().NotBeNull();
        id.Should().NotBeEmpty();

        ObjectId.TryParse(id, out _).Should().BeTrue();
    }

    [Fact]
    public async Task Register_Response_ShouldContainProfileFields()
    {
        var dto = new RegisterDto
        {
            Email = "response-profile@example.com",
            Password = "pass123",
            FullName = "Response User",
            PhoneNumber = "0701234567",
            ShippingAddress = new Address
            {
                Street = "Street 1",
                City = "Gothenburg",
                PostalCode = "41101",
                Country = "Sweden"
            }
        };

        var result = await _controller.Register(dto);

        var ok = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var value = ok.Value!;

        value.GetType().GetProperty("phoneNumber")
            .Should().NotBeNull();

        value.GetType().GetProperty("shippingAddress")
            .Should().NotBeNull();

        value.GetType().GetProperty("billingAddress")
            .Should().NotBeNull();

        value.GetType().GetProperty("createdAt")
            .Should().NotBeNull();

        value.GetType().GetProperty("updatedAt")
            .Should().NotBeNull();

        value.GetType().GetProperty("isEmailVerified")
            .Should().NotBeNull();
    }

    [Fact]
    public async Task Register_Response_ShouldNotExposePasswordHash()
    {
        var dto = new RegisterDto
        {
            Email = "secure-response@example.com",
            Password = "pass123",
            FullName = "Secure User"
        };

        var result = await _controller.Register(dto);

        var ok = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        ok.Value!.GetType()
            .GetProperty("passwordHash")
            .Should()
            .BeNull();
    }

    // =========================================================
    // LOGIN - VALIDATION
    // =========================================================

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
    public async Task Login_ShouldReturnBadRequest_WhenEmailWhitespace()
    {
        var dto = new LoginDto
        {
            Email = "   ",
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
    public async Task Login_ShouldReturnBadRequest_WhenPasswordWhitespace()
    {
        var dto = new LoginDto
        {
            Email = "test@example.com",
            Password = "   "
        };

        var result = await _controller.Login(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenPasswordNull()
    {
        var dto = new LoginDto
        {
            Email = "test@example.com",
            Password = null!
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

    // =========================================================
    // LOGIN - INVALID CREDENTIALS
    // =========================================================

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
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordHashIsInvalid()
    {
        var user = new User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = "invalid-hash@example.com",
            FullName = "Invalid Hash User",
            PasswordHash = "not-a-valid-bcrypt-hash",
            Roles = new List<string> { "User" }
        };

        await _userRepo.CreateAsync(user);

        var dto = new LoginDto
        {
            Email = "invalid-hash@example.com",
            Password = "pass123"
        };

        var result = await _controller.Login(dto);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserHasDeletedAccount()
    {
        var registerDto = new RegisterDto
        {
            Email = "deleted@example.com",
            Password = "pass123",
            FullName = "Deleted User"
        };

        var registerResult = await _controller.Register(registerDto);

        registerResult.Should().BeOfType<OkObjectResult>();

        var user = await _userRepo.GetByEmailAsync("deleted@example.com");

        user.Should().NotBeNull();

        await _userRepo.DeleteAsync(user!.Id);

        var loginDto = new LoginDto
        {
            Email = "deleted@example.com",
            Password = "pass123"
        };

        var result = await _controller.Login(loginDto);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // =========================================================
    // LOGIN - SUCCESS
    // =========================================================

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

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var registerDto = new RegisterDto
        {
            Email = "token@example.com",
            Password = "pass123",
            FullName = "Token User"
        };

        await _controller.Register(registerDto);

        var loginDto = new LoginDto
        {
            Email = "token@example.com",
            Password = "pass123"
        };

        var result = await _controller.Login(loginDto);

        var ok = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        ok.Value.Should().NotBeNull();

        var token = ok.Value!
            .GetType()
            .GetProperty("token")!
            .GetValue(ok.Value)
            ?.ToString();

        token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();

        handler.CanReadToken(token!)
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task Login_Response_ShouldContainExpiration()
    {
        var registerDto = new RegisterDto
        {
            Email = "expiration@example.com",
            Password = "pass123",
            FullName = "Expiration User"
        };

        await _controller.Register(registerDto);

        var loginDto = new LoginDto
        {
            Email = "expiration@example.com",
            Password = "pass123"
        };

        var result = await _controller.Login(loginDto);

        var ok = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        ok.Value!.GetType()
            .GetProperty("expires")
            .Should()
            .NotBeNull();
    }

    [Fact]
    public async Task Login_Token_ShouldContainUserId()
    {
        var registerDto = new RegisterDto
        {
            Email = "claims@example.com",
            Password = "pass123",
            FullName = "Claims User"
        };

        await _controller.Register(registerDto);

        var user = await _userRepo.GetByEmailAsync(
            "claims@example.com");

        user.Should().NotBeNull();

        var loginDto = new LoginDto
        {
            Email = "claims@example.com",
            Password = "pass123"
        };

        var result = await _controller.Login(loginDto);

        var ok = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var token = ok.Value!
            .GetType()
            .GetProperty("token")!
            .GetValue(ok.Value)
            ?.ToString();

        token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token!);

        var sub = jwt.Claims
            .FirstOrDefault(c => c.Type == "sub")
            ?.Value;

        sub.Should().Be(user!.Id);
    }

    // =========================================================
    // LOGIN - EMAIL CASE
    // =========================================================

    [Fact]
    public async Task Login_ShouldReturnOk_WhenEmailCaseDiffers()
    {
        var registerDto = new RegisterDto
        {
            Email = "case@example.com",
            Password = "pass123",
            FullName = "Case User"
        };

        await _controller.Register(registerDto);

        var loginDto = new LoginDto
        {
            Email = "CASE@EXAMPLE.COM",
            Password = "pass123"
        };

        var result = await _controller.Login(loginDto);

        result.Should().BeOfType<OkObjectResult>();
    }

    // =========================================================
    // LOGOUT - AUTHENTICATION
    // =========================================================

    [Fact]
    public async Task Logout_ShouldReturnUnauthorized_WhenUserIdClaimMissing()
    {
        var claims = new List<Claim>();

        SetAuthenticatedUser(claims, "Bearer some-token");

        var result = await _controller.Logout();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Logout_ShouldReturnUnauthorized_WhenUserIdClaimEmpty()
    {
        var claims = new List<Claim>
        {
            new Claim("id", "")
        };

        SetAuthenticatedUser(claims, "Bearer some-token");

        var result = await _controller.Logout();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Logout_ShouldReturnUnauthorized_WhenAuthorizationHeaderMissing()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var claims = new List<Claim>
        {
            new Claim("id", userId)
        };

        SetAuthenticatedUser(claims, null);

        var result = await _controller.Logout();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Logout_ShouldReturnUnauthorized_WhenAuthorizationHeaderEmpty()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var claims = new List<Claim>
        {
            new Claim("id", userId)
        };

        SetAuthenticatedUser(claims, "");

        var result = await _controller.Logout();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Logout_ShouldReturnOk_WhenValidToken()
    {
        var registerDto = new RegisterDto
        {
            Email = "logout@example.com",
            Password = "pass123",
            FullName = "Logout User"
        };

        await _controller.Register(registerDto);

        var user = await _userRepo.GetByEmailAsync(
            "logout@example.com");

        user.Should().NotBeNull();

        var token = await _authService.LoginAsync(
            "logout@example.com",
            "pass123");

        token.Should().NotBeNullOrWhiteSpace();

        var claims = new List<Claim>
        {
            new Claim("id", user!.Id)
        };

        SetAuthenticatedUser(
            claims,
            $"Bearer {token}");

        var result = await _controller.Logout();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Logout_ShouldAcceptBearerTokenWithWhitespace()
    {
        var registerDto = new RegisterDto
        {
            Email = "logout-space@example.com",
            Password = "pass123",
            FullName = "Logout User"
        };

        await _controller.Register(registerDto);

        var user = await _userRepo.GetByEmailAsync(
            "logout-space@example.com");

        user.Should().NotBeNull();

        var token = await _authService.LoginAsync(
            "logout-space@example.com",
            "pass123");

        token.Should().NotBeNullOrWhiteSpace();

        var claims = new List<Claim>
        {
            new Claim("id", user!.Id)
        };

        SetAuthenticatedUser(
            claims,
            $"Bearer    {token}   ");

        var result = await _controller.Logout();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Logout_ShouldReturnOk_WhenCalledTwice()
    {
        var registerDto = new RegisterDto
        {
            Email = "logout-twice@example.com",
            Password = "pass123",
            FullName = "Logout Twice"
        };

        await _controller.Register(registerDto);

        var user = await _userRepo.GetByEmailAsync(
            "logout-twice@example.com");

        user.Should().NotBeNull();

        var token = await _authService.LoginAsync(
            "logout-twice@example.com",
            "pass123");

        token.Should().NotBeNullOrWhiteSpace();

        var claims = new List<Claim>
        {
            new Claim("id", user!.Id)
        };

        SetAuthenticatedUser(
            claims,
            $"Bearer {token}");

        var firstResult = await _controller.Logout();
        var secondResult = await _controller.Logout();

        firstResult.Should().BeOfType<OkObjectResult>();
        secondResult.Should().BeOfType<OkObjectResult>();
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private void SetAuthenticatedUser(
        IEnumerable<Claim> claims,
        string? authorizationHeader)
    {
        var identity = new ClaimsIdentity(
            claims,
            "TestAuthentication");

        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };

        if (authorizationHeader != null)
        {
            _controller.ControllerContext.HttpContext.Request.Headers[
                "Authorization"] = authorizationHeader;
        }
    }
}
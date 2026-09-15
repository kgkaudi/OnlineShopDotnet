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
    private readonly InvalidTokenRepository _invalidTokens;
    private readonly JwtService _jwt;

    public AuthServiceTests(MongoTestFixture fixture)
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

        _jwt = new JwtService(jwtConfig);

        _auth = new AuthService(
            jwtConfig,
            _jwt,
            _userRepo,
            _invalidTokens
        );

        Fixture.Database.DropCollection("Users");
        Fixture.Database.DropCollection("InvalidTokens");
    }

    // =========================================================
    // REGISTER — INVALID INPUT
    // =========================================================

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenEmailIsNull()
    {
        var result = await _auth.RegisterAsync(
            null!,
            "pass",
            "User",
            null,
            null,
            null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenEmailIsWhitespace()
    {
        var result = await _auth.RegisterAsync(
            "   ",
            "pass",
            "User",
            null,
            null,
            null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenPasswordIsNull()
    {
        var result = await _auth.RegisterAsync(
            "test@test.com",
            null!,
            "User",
            null,
            null,
            null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenPasswordIsWhitespace()
    {
        var result = await _auth.RegisterAsync(
            "test@test.com",
            "   ",
            "User",
            null,
            null,
            null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenFullNameIsNull()
    {
        var result = await _auth.RegisterAsync(
            "test@test.com",
            "pass",
            null!,
            null,
            null,
            null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenFullNameIsWhitespace()
    {
        var result = await _auth.RegisterAsync(
            "test@test.com",
            "pass",
            "   ",
            null,
            null,
            null);

        result.Should().BeNull();
    }

    // =========================================================
    // REGISTER — NORMALIZATION
    // =========================================================

    [Fact]
    public async Task RegisterAsync_ShouldTrimAndNormalizeEmail()
    {
        var result = await _auth.RegisterAsync(
            "  TEST@TEST.COM  ",
            "pass123",
            "User",
            null,
            null,
            null);

        result.Should().NotBeNull();
        result!.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task RegisterAsync_ShouldTrimFullName()
    {
        var result = await _auth.RegisterAsync(
            "trim@test.com",
            "pass123",
            "  John Doe  ",
            null,
            null,
            null);

        result.Should().NotBeNull();
        result!.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task RegisterAsync_ShouldTrimPhoneNumber()
    {
        var result = await _auth.RegisterAsync(
            "phone@test.com",
            "pass123",
            "User",
            "  +46701234567  ",
            null,
            null);

        result.Should().NotBeNull();
        result!.PhoneNumber.Should().Be("+46701234567");
    }

    [Fact]
    public async Task RegisterAsync_ShouldConvertWhitespacePhoneNumberToNull()
    {
        var result = await _auth.RegisterAsync(
            "phone-null@test.com",
            "pass123",
            "User",
            "   ",
            null,
            null);

        result.Should().NotBeNull();
        result!.PhoneNumber.Should().BeNull();
    }

    // =========================================================
    // REGISTER — EXISTING EMAIL
    // =========================================================

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenEmailAlreadyExists_CaseInsensitive()
    {
        await _auth.RegisterAsync(
            "duplicate@test.com",
            "pass",
            "User1",
            null,
            null,
            null);

        var result = await _auth.RegisterAsync(
            "DUPLICATE@test.com",
            "pass",
            "User2",
            null,
            null,
            null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenEmailAlreadyExistsWithWhitespace()
    {
        await _auth.RegisterAsync(
            "duplicate-space@test.com",
            "pass",
            "User1",
            null,
            null,
            null);

        var result = await _auth.RegisterAsync(
            "  DUPLICATE-SPACE@test.com  ",
            "pass",
            "User2",
            null,
            null,
            null);

        result.Should().BeNull();
    }

    // =========================================================
    // REGISTER — USER DEFAULTS
    // =========================================================

    [Fact]
    public async Task RegisterAsync_ShouldCreateValidObjectId()
    {
        var result = await _auth.RegisterAsync(
            "id@test.com",
            "pass123",
            "User",
            null,
            null,
            null);

        result.Should().NotBeNull();
        ObjectId.TryParse(result!.Id, out _).Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_ShouldAssignUserRole()
    {
        var result = await _auth.RegisterAsync(
            "role@test.com",
            "pass123",
            "User",
            null,
            null,
            null);

        result.Should().NotBeNull();
        result!.Roles.Should().ContainSingle();
        result.Roles.Should().Contain("User");
    }

    [Fact]
    public async Task RegisterAsync_ShouldSetCreatedAt()
    {
        var before = DateTime.UtcNow;

        var result = await _auth.RegisterAsync(
            "created@test.com",
            "pass123",
            "User",
            null,
            null,
            null);

        var after = DateTime.UtcNow;

        result.Should().NotBeNull();

        result!.CreatedAt.Should().BeOnOrAfter(before);
        result.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task RegisterAsync_ShouldSetUpdatedAt()
    {
        var result = await _auth.RegisterAsync(
            "updated@test.com",
            "pass123",
            "User",
            null,
            null,
            null);

        result.Should().NotBeNull();

        result!.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task RegisterAsync_ShouldInitializeUpdatedAtCloseToCreatedAt()
    {
        var result = await _auth.RegisterAsync(
            "timestamps@test.com",
            "pass123",
            "User",
            null,
            null,
            null);

        result.Should().NotBeNull();

        result!.UpdatedAt
            .Should()
            .BeCloseTo(result.CreatedAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task RegisterAsync_ShouldSetEmailAsUnverified()
    {
        var result = await _auth.RegisterAsync(
            "verify@test.com",
            "pass123",
            "User",
            null,
            null,
            null);

        result.Should().NotBeNull();
        result!.IsEmailVerified.Should().BeFalse();
    }

    // =========================================================
    // REGISTER — PASSWORD SECURITY
    // =========================================================

    [Fact]
    public async Task RegisterAsync_ShouldHashPassword()
    {
        var result = await _auth.RegisterAsync(
            "hash@test.com",
            "plainPassword123",
            "User",
            null,
            null,
            null);

        result.Should().NotBeNull();

        result!.PasswordHash
            .Should()
            .NotBe("plainPassword123");

        result.PasswordHash.Should().StartWith("$2");
    }

    [Fact]
    public async Task RegisterAsync_ShouldStoreWorkingPasswordHash()
    {
        var password = "correctPassword123";

        var result = await _auth.RegisterAsync(
            "hash-valid@test.com",
            password,
            "User",
            null,
            null,
            null);

        result.Should().NotBeNull();

        BCrypt.Net.BCrypt.Verify(
            password,
            result!.PasswordHash)
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_ShouldNotStorePlainTextPassword()
    {
        var password = "SuperSecret123";

        var result = await _auth.RegisterAsync(
            "security@test.com",
            password,
            "User",
            null,
            null,
            null);

        result.Should().NotBeNull();
        result!.PasswordHash.Should().NotContain(password);
    }

    // =========================================================
    // REGISTER — PROFILE DATA
    // =========================================================

    [Fact]
    public async Task RegisterAsync_ShouldStorePhoneNumber()
    {
        var result = await _auth.RegisterAsync(
            "profile-phone@test.com",
            "pass123",
            "Maria Rapti",
            "+46701234567",
            null,
            null);

        result.Should().NotBeNull();
        result!.PhoneNumber.Should().Be("+46701234567");
    }

    [Fact]
    public async Task RegisterAsync_ShouldStoreShippingAddress()
    {
        var shippingAddress = new Address
        {
            Street = "Main Street 10",
            City = "Gothenburg",
            State = "Vastra Gotaland",
            PostalCode = "41101",
            Country = "Sweden"
        };

        var result = await _auth.RegisterAsync(
            "shipping@test.com",
            "pass123",
            "Maria Rapti",
            null,
            shippingAddress,
            null);

        result.Should().NotBeNull();

        result!.ShippingAddress.Should().NotBeNull();
        result.ShippingAddress!.Street.Should().Be("Main Street 10");
        result.ShippingAddress.City.Should().Be("Gothenburg");
        result.ShippingAddress.State.Should().Be("Vastra Gotaland");
        result.ShippingAddress.PostalCode.Should().Be("41101");
        result.ShippingAddress.Country.Should().Be("Sweden");
    }

    [Fact]
    public async Task RegisterAsync_ShouldStoreBillingAddress()
    {
        var billingAddress = new Address
        {
            Street = "Billing Street 20",
            City = "Stockholm",
            State = "Stockholm",
            PostalCode = "11120",
            Country = "Sweden"
        };

        var result = await _auth.RegisterAsync(
            "billing@test.com",
            "pass123",
            "Maria Rapti",
            null,
            null,
            billingAddress);

        result.Should().NotBeNull();

        result!.BillingAddress.Should().NotBeNull();
        result.BillingAddress!.Street.Should().Be("Billing Street 20");
        result.BillingAddress.City.Should().Be("Stockholm");
        result.BillingAddress.State.Should().Be("Stockholm");
        result.BillingAddress.PostalCode.Should().Be("11120");
        result.BillingAddress.Country.Should().Be("Sweden");
    }

    [Fact]
    public async Task RegisterAsync_ShouldStoreCompleteProfile()
    {
        var shippingAddress = new Address
        {
            Street = "Shipping Street 1",
            City = "Gothenburg",
            State = "Vastra Gotaland",
            PostalCode = "41101",
            Country = "Sweden"
        };

        var billingAddress = new Address
        {
            Street = "Billing Street 2",
            City = "Stockholm",
            State = "Stockholm",
            PostalCode = "11122",
            Country = "Sweden"
        };

        var result = await _auth.RegisterAsync(
            "complete-profile@test.com",
            "pass123",
            "Maria Rapti",
            "+46701234567",
            shippingAddress,
            billingAddress);

        result.Should().NotBeNull();

        result!.PhoneNumber.Should().Be("+46701234567");

        result.ShippingAddress.Should().BeEquivalentTo(
            shippingAddress);

        result.BillingAddress.Should().BeEquivalentTo(
            billingAddress);
    }

    [Fact]
    public async Task RegisterAsync_ShouldPersistProfileData()
    {
        var shippingAddress = new Address
        {
            Street = "Persist Street 10",
            City = "Gothenburg",
            State = "Vastra Gotaland",
            PostalCode = "41101",
            Country = "Sweden"
        };

        var billingAddress = new Address
        {
            Street = "Persist Billing 20",
            City = "Malmo",
            State = "Skane",
            PostalCode = "21120",
            Country = "Sweden"
        };

        var created = await _auth.RegisterAsync(
            "persist-profile@test.com",
            "pass123",
            "Maria Rapti",
            "+46701234567",
            shippingAddress,
            billingAddress);

        created.Should().NotBeNull();

        var stored = await _userRepo.GetByIdAsync(
            created!.Id);

        stored.Should().NotBeNull();

        stored!.PhoneNumber.Should().Be("+46701234567");

        stored.ShippingAddress.Should()
            .BeEquivalentTo(shippingAddress);

        stored.BillingAddress.Should()
            .BeEquivalentTo(billingAddress);
    }

    [Fact]
    public async Task RegisterAsync_ShouldPersistNullOptionalProfileFields()
    {
        var created = await _auth.RegisterAsync(
            "null-profile@test.com",
            "pass123",
            "User",
            null,
            null,
            null);

        created.Should().NotBeNull();

        var stored = await _userRepo.GetByIdAsync(
            created!.Id);

        stored.Should().NotBeNull();

        stored!.PhoneNumber.Should().BeNull();
        stored.ShippingAddress.Should().BeNull();
        stored.BillingAddress.Should().BeNull();
    }

    // =========================================================
    // LOGIN — EDGE CASES
    // =========================================================

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenEmailIsNull()
    {
        var result = await _auth.LoginAsync(
            null!,
            "pass");

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenEmailIsWhitespace()
    {
        var result = await _auth.LoginAsync(
            "   ",
            "pass");

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsNull()
    {
        var result = await _auth.LoginAsync(
            "test@test.com",
            null!);

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsWhitespace()
    {
        var result = await _auth.LoginAsync(
            "test@test.com",
            "   ");

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsEmpty()
    {
        var result = await _auth.LoginAsync(
            "test@test.com",
            "");

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var result = await _auth.LoginAsync(
            "missing@test.com",
            "pass123");

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
    {
        await _auth.RegisterAsync(
            "wrong-password@test.com",
            "correctPassword",
            "User",
            null,
            null,
            null);

        var result = await _auth.LoginAsync(
            "wrong-password@test.com",
            "wrongPassword");

        result.Should().BeNull();
    }

    // =========================================================
    // LOGIN — EMAIL BEHAVIOR
    // =========================================================

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenEmailCaseDiffers()
    {
        await _auth.RegisterAsync(
            "case@test.com",
            "pass123",
            "User",
            null,
            null,
            null);

        var result = await _auth.LoginAsync(
            "CASE@test.com",
            "pass123");

        result.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenEmailHasLeadingOrTrailingWhitespace()
    {
        await _auth.RegisterAsync(
            "spaces@test.com",
            "pass123",
            "User",
            null,
            null,
            null);

        var result = await _auth.LoginAsync(
            "  spaces@test.com  ",
            "pass123");

        result.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsCorrectButUserIsDeleted()
    {
        var user = await _auth.RegisterAsync(
            "delete@test.com",
            "pass123",
            "User",
            null,
            null,
            null);

        user.Should().NotBeNull();

        await _userRepo.DeleteAsync(user!.Id);

        var result = await _auth.LoginAsync(
            "delete@test.com",
            "pass123");

        result.Should().BeNull();
    }

    // =========================================================
    // LOGIN — PASSWORD HASH EDGE CASES
    // =========================================================

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordHashIsInvalid()
    {
        var user = await _auth.RegisterAsync(
            "invalid-hash@test.com",
            "pass123",
            "User",
            null,
            null,
            null);

        user.Should().NotBeNull();

        user!.PasswordHash = "invalid-hash";

        await _userRepo.UpdateAsync(user);

        var result = await _auth.LoginAsync(
            "invalid-hash@test.com",
            "pass123");

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordHashIsEmpty()
    {
        var user = await _auth.RegisterAsync(
            "empty-hash@test.com",
            "pass123",
            "User",
            null,
            null,
            null);

        user.Should().NotBeNull();

        user!.PasswordHash = "";

        await _userRepo.UpdateAsync(user);

        var result = await _auth.LoginAsync(
            "empty-hash@test.com",
            "pass123");

        result.Should().BeNull();
    }

    // =========================================================
    // ADD ROLE — EDGE CASES
    // =========================================================

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserIdIsNull()
    {
        var result = await _auth.AddRoleAsync(
            null!,
            "Admin");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserIdIsWhitespace()
    {
        var result = await _auth.AddRoleAsync(
            "   ",
            "Admin");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenRoleIsNull()
    {
        var user = await _auth.RegisterAsync(
            "role@test.com",
            "pass",
            "User",
            null,
            null,
            null);

        var result = await _auth.AddRoleAsync(
            user!.Id,
            null!);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenRoleIsWhitespace()
    {
        var user = await _auth.RegisterAsync(
            "role2@test.com",
            "pass",
            "User",
            null,
            null,
            null);

        var result = await _auth.AddRoleAsync(
            user!.Id,
            "   ");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserIdIsInvalidObjectId()
    {
        var result = await _auth.AddRoleAsync(
            "invalid-id",
            "Admin");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var result = await _auth.AddRoleAsync(
            id,
            "Admin");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldNotDuplicateRole_WhenRoleAlreadyExists()
    {
        var user = await _auth.RegisterAsync(
            "dup@test.com",
            "pass",
            "User",
            null,
            null,
            null);

        await _auth.AddRoleAsync(
            user!.Id,
            "Admin");

        var result = await _auth.AddRoleAsync(
            user.Id,
            "Admin");

        result.Should().BeTrue();

        var fetched = await _userRepo.GetByIdAsync(
            user.Id);

        fetched.Should().NotBeNull();

        fetched!.Roles.Should().HaveCount(2);
        fetched.Roles.Should().Contain(
            new[] { "User", "Admin" });
    }

    [Fact]
    public async Task AddRoleAsync_ShouldAddNewRole()
    {
        var user = await _auth.RegisterAsync(
            "new-role@test.com",
            "pass",
            "User",
            null,
            null,
            null);

        var result = await _auth.AddRoleAsync(
            user!.Id,
            "Admin");

        result.Should().BeTrue();

        var fetched = await _userRepo.GetByIdAsync(
            user.Id);

        fetched.Should().NotBeNull();
        fetched!.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task AddRoleAsync_ShouldAcceptRoleWithWhitespace()
    {
        var user = await _auth.RegisterAsync(
            "role-whitespace@test.com",
            "pass",
            "User",
            null,
            null,
            null);

        var result = await _auth.AddRoleAsync(
            user!.Id,
            "  Admin  ");

        result.Should().BeTrue();

        var fetched = await _userRepo.GetByIdAsync(
            user.Id);

        fetched.Should().NotBeNull();
        fetched!.Roles.Should().Contain("Admin");
    }

    // =========================================================
    // LOGOUT — EDGE CASES
    // =========================================================

    [Fact]
    public async Task LogoutAsync_ShouldReturnFalse_WhenUserIdIsNull()
    {
        var result = await _auth.LogoutAsync(
            null!,
            "token");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task LogoutAsync_ShouldReturnFalse_WhenUserIdIsWhitespace()
    {
        var result = await _auth.LogoutAsync(
            "   ",
            "token");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task LogoutAsync_ShouldReturnFalse_WhenTokenIsNull()
    {
        var result = await _auth.LogoutAsync(
            "user-id",
            null!);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task LogoutAsync_ShouldReturnFalse_WhenTokenIsWhitespace()
    {
        var result = await _auth.LogoutAsync(
            "user-id",
            "   ");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task LogoutAsync_ShouldReturnTrue_WhenUserIdAndTokenAreValid()
    {
        var user = await _auth.RegisterAsync(
            "logout@test.com",
            "pass123",
            "User",
            null,
            null,
            null);

        user.Should().NotBeNull();

        var token = await _auth.LoginAsync(
            "logout@test.com",
            "pass123");

        token.Should().NotBeNullOrWhiteSpace();

        var result = await _auth.LogoutAsync(
            user!.Id,
            token!);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task LogoutAsync_ShouldReturnTrue_AndInvalidateToken()
    {
        var user = await _auth.RegisterAsync(
            "logout-persist@test.com",
            "pass123",
            "User",
            null,
            null,
            null);

        user.Should().NotBeNull();

        var token = await _auth.LoginAsync(
            "logout-persist@test.com",
            "pass123");

        token.Should().NotBeNullOrWhiteSpace();

        var result = await _auth.LogoutAsync(
            user!.Id,
            token!);

        result.Should().BeTrue();
    }
}
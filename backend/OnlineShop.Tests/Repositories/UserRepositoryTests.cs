using FluentAssertions;
using MongoDB.Bson;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using Xunit;

public class UserRepositoryTests : RepositoryTestBase
{
    private readonly UserRepository _repo;

    public UserRepositoryTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new UserRepository(config);
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoUsers()
    {
        var result = await _repo.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        await _repo.CreateAsync(
            new User
            {
                Email = "a@test.com",
                PasswordHash = "x"
            });

        await _repo.CreateAsync(
            new User
            {
                Email = "b@test.com",
                PasswordHash = "y"
            });

        var result = await _repo.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldPreserveNewUserFields()
    {
        var user = CreateUser();

        await _repo.CreateAsync(user);

        var result = await _repo.GetAllAsync();

        result.Should().ContainSingle();

        var fetched = result.Single();

        fetched.PhoneNumber.Should().Be(user.PhoneNumber);
        fetched.ShippingAddress.Should().NotBeNull();
        fetched.BillingAddress.Should().NotBeNull();
        fetched.IsEmailVerified.Should().BeTrue();
        fetched.CreatedAt.Should().Be(user.CreatedAt);
        fetched.UpdatedAt.Should().Be(user.UpdatedAt);
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsNull()
    {
        var result = await _repo.GetByIdAsync(null!);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsWhitespace()
    {
        var result = await _repo.GetByIdAsync("   ");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsInvalid()
    {
        var result = await _repo.GetByIdAsync("invalid-id");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
    {
        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = "x"
        };

        await _repo.CreateAsync(user);

        var result = await _repo.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repo.GetByIdAsync(
            ObjectId.GenerateNewId().ToString());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldPreserveShippingAndBillingAddresses()
    {
        var user = CreateUser();

        await _repo.CreateAsync(user);

        var result = await _repo.GetByIdAsync(user.Id);

        result.Should().NotBeNull();

        result!.ShippingAddress.Should().NotBeNull();
        result.BillingAddress.Should().NotBeNull();

        result.ShippingAddress!.Street.Should().Be("Shipping Street 1");
        result.ShippingAddress.City.Should().Be("Gothenburg");
        result.ShippingAddress.PostalCode.Should().Be("411 01");
        result.ShippingAddress.Country.Should().Be("Sweden");

        result.BillingAddress!.Street.Should().Be("Billing Street 2");
        result.BillingAddress.City.Should().Be("Stockholm");
        result.BillingAddress.PostalCode.Should().Be("111 01");
        result.BillingAddress.Country.Should().Be("Sweden");
    }

    // ---------------------------------------------------------
    // GET BY EMAIL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailIsNull()
    {
        var result = await _repo.GetByEmailAsync(null!);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailIsWhitespace()
    {
        var result = await _repo.GetByEmailAsync("   ");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenExists()
    {
        var user = new User
        {
            Email = "email@test.com",
            PasswordHash = "x"
        };

        await _repo.CreateAsync(user);

        var result = await _repo.GetByEmailAsync("email@test.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("email@test.com");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repo.GetByEmailAsync("missing@test.com");

        result.Should().BeNull();
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUserIsNull()
    {
        Func<Task> act = async () =>
            await _repo.CreateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenEmailIsMissing()
    {
        var user = new User
        {
            Email = null!,
            PasswordHash = "x"
        };

        Func<Task> act = async () =>
            await _repo.CreateAsync(user);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenEmailIsWhitespace()
    {
        var user = new User
        {
            Email = "   ",
            PasswordHash = "x"
        };

        Func<Task> act = async () =>
            await _repo.CreateAsync(user);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenPasswordHashIsMissing()
    {
        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = null!
        };

        Func<Task> act = async () =>
            await _repo.CreateAsync(user);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenPasswordHashIsWhitespace()
    {
        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = "   "
        };

        Func<Task> act = async () =>
            await _repo.CreateAsync(user);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldInsertUser()
    {
        var user = new User
        {
            Email = "create@test.com",
            PasswordHash = "x"
        };

        await _repo.CreateAsync(user);

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched.Should().NotBeNull();
        fetched!.Email.Should().Be("create@test.com");
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateId_WhenIdIsMissing()
    {
        var user = new User
        {
            Id = "",
            Email = "generated@test.com",
            PasswordHash = "x"
        };

        await _repo.CreateAsync(user);

        user.Id.Should().NotBeNullOrWhiteSpace();
        ObjectId.TryParse(user.Id, out _).Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenIdIsInvalid()
    {
        var user = new User
        {
            Id = "invalid-id",
            Email = "invalid-id@test.com",
            PasswordHash = "x"
        };

        Func<Task> act = async () =>
            await _repo.CreateAsync(user);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistPhoneNumber()
    {
        var user = new User
        {
            Email = "phone@test.com",
            PasswordHash = "x",
            PhoneNumber = "+46 70 123 45 67"
        };

        await _repo.CreateAsync(user);

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched.Should().NotBeNull();
        fetched!.PhoneNumber.Should().Be("+46 70 123 45 67");
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistNullPhoneNumber()
    {
        var user = new User
        {
            Email = "no-phone@test.com",
            PasswordHash = "x",
            PhoneNumber = null
        };

        await _repo.CreateAsync(user);

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched.Should().NotBeNull();
        fetched!.PhoneNumber.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistAddresses()
    {
        var user = CreateUser();

        await _repo.CreateAsync(user);

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched.Should().NotBeNull();

        fetched!.ShippingAddress.Should().NotBeNull();
        fetched.BillingAddress.Should().NotBeNull();

        fetched.ShippingAddress!.Street
            .Should().Be("Shipping Street 1");

        fetched.BillingAddress!.Street
            .Should().Be("Billing Street 2");
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistEmailVerificationStatus()
    {
        var user = new User
        {
            Email = "verified@test.com",
            PasswordHash = "x",
            IsEmailVerified = true
        };

        await _repo.CreateAsync(user);

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched.Should().NotBeNull();
        fetched!.IsEmailVerified.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistCreatedAt()
    {
        var createdAt = DateTime.UtcNow;

        var user = new User
        {
            Email = "created-at@test.com",
            PasswordHash = "x",
            CreatedAt = createdAt
        };

        await _repo.CreateAsync(user);

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched.Should().NotBeNull();

        fetched!.CreatedAt.Should()
            .BeCloseTo(createdAt, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistUpdatedAt()
    {
        var updatedAt = DateTime.UtcNow;

        var user = new User
        {
            Email = "updated-at@test.com",
            PasswordHash = "x",
            UpdatedAt = updatedAt
        };

        await _repo.CreateAsync(user);

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched.Should().NotBeNull();

        fetched!.UpdatedAt.Should()
            .BeCloseTo(updatedAt, TimeSpan.FromMilliseconds(100));
    }

    // ---------------------------------------------------------
    // ADD ROLE
    // ---------------------------------------------------------

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserIdIsNull()
    {
        var result = await _repo.AddRoleAsync(null!, "Admin");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserIdIsWhitespace()
    {
        var result = await _repo.AddRoleAsync("   ", "Admin");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserIdIsInvalid()
    {
        var result = await _repo.AddRoleAsync(
            "invalid-id",
            "Admin");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenRoleIsNull()
    {
        var user = new User
        {
            Email = "role@test.com",
            PasswordHash = "x"
        };

        await _repo.CreateAsync(user);

        var result = await _repo.AddRoleAsync(
            user.Id,
            null!);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenRoleIsWhitespace()
    {
        var user = new User
        {
            Email = "role@test.com",
            PasswordHash = "x"
        };

        await _repo.CreateAsync(user);

        var result = await _repo.AddRoleAsync(
            user.Id,
            "   ");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldAddRoleToUser()
    {
        var user = new User
        {
            Email = "role@test.com",
            PasswordHash = "x",
            Roles = new List<string>()
        };

        await _repo.CreateAsync(user);

        var updated = await _repo.AddRoleAsync(
            user.Id,
            "Admin");

        updated.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched!.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task AddRoleAsync_ShouldNotDuplicateRole()
    {
        var user = new User
        {
            Email = "role2@test.com",
            PasswordHash = "x",
            Roles = new List<string> { "Admin" }
        };

        await _repo.CreateAsync(user);

        var updated = await _repo.AddRoleAsync(
            user.Id,
            "Admin");

        updated.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched!.Roles.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        var result = await _repo.AddRoleAsync(
            ObjectId.GenerateNewId().ToString(),
            "Admin");

        result.Should().BeFalse();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenUserIsNull()
    {
        var result = await _repo.UpdateAsync(null!);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenUserIdIsNull()
    {
        var user = new User
        {
            Id = null!,
            Email = "update@test.com",
            PasswordHash = "x"
        };

        var result = await _repo.UpdateAsync(user);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenUserIdIsWhitespace()
    {
        var user = new User
        {
            Id = "   ",
            Email = "update@test.com",
            PasswordHash = "x"
        };

        var result = await _repo.UpdateAsync(user);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenUserIdIsInvalid()
    {
        var user = new User
        {
            Id = "invalid-id",
            Email = "update@test.com",
            PasswordHash = "x"
        };

        var result = await _repo.UpdateAsync(user);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        var user = new User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = "missing@test.com",
            PasswordHash = "x"
        };

        var result = await _repo.UpdateAsync(user);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        var user = new User
        {
            Email = "before@test.com",
            PasswordHash = "old"
        };

        await _repo.CreateAsync(user);

        user.Email = "after@test.com";
        user.FullName = "Updated User";

        var result = await _repo.UpdateAsync(user);

        result.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched.Should().NotBeNull();
        fetched!.Email.Should().Be("after@test.com");
        fetched.FullName.Should().Be("Updated User");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePhoneNumber()
    {
        var user = new User
        {
            Email = "update-phone@test.com",
            PasswordHash = "x"
        };

        await _repo.CreateAsync(user);

        user.PhoneNumber = "+46 70 987 65 43";

        var result = await _repo.UpdateAsync(user);

        result.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched!.PhoneNumber.Should().Be("+46 70 987 65 43");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateShippingAddress()
    {
        var user = new User
        {
            Email = "update-shipping@test.com",
            PasswordHash = "x"
        };

        await _repo.CreateAsync(user);

        user.ShippingAddress = new Address
        {
            Street = "New Shipping Street",
            City = "Gothenburg",
            State = "Västra Götaland",
            PostalCode = "411 10",
            Country = "Sweden"
        };

        var result = await _repo.UpdateAsync(user);

        result.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched!.ShippingAddress.Should().NotBeNull();
        fetched.ShippingAddress!.Street
            .Should().Be("New Shipping Street");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateBillingAddress()
    {
        var user = new User
        {
            Email = "update-billing@test.com",
            PasswordHash = "x"
        };

        await _repo.CreateAsync(user);

        user.BillingAddress = new Address
        {
            Street = "New Billing Street",
            City = "Stockholm",
            State = "Stockholm",
            PostalCode = "111 10",
            Country = "Sweden"
        };

        var result = await _repo.UpdateAsync(user);

        result.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched!.BillingAddress.Should().NotBeNull();
        fetched.BillingAddress!.Street
            .Should().Be("New Billing Street");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEmailVerificationStatus()
    {
        var user = new User
        {
            Email = "verification@test.com",
            PasswordHash = "x",
            IsEmailVerified = false
        };

        await _repo.CreateAsync(user);

        user.IsEmailVerified = true;

        var result = await _repo.UpdateAsync(user);

        result.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched!.IsEmailVerified.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPreservePasswordHash()
    {
        var user = new User
        {
            Email = "password@test.com",
            PasswordHash = "original-hash"
        };

        await _repo.CreateAsync(user);

        user.FullName = "Updated Name";

        var result = await _repo.UpdateAsync(user);

        result.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched!.PasswordHash.Should().Be("original-hash");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUpdatedAt()
    {
        var user = new User
        {
            Email = "timestamp@test.com",
            PasswordHash = "x"
        };

        await _repo.CreateAsync(user);

        var oldUpdatedAt = user.UpdatedAt;

        await Task.Delay(10);

        var result = await _repo.UpdateAsync(user);

        result.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched!.UpdatedAt.Should().BeAfter(oldUpdatedAt);
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsNull()
    {
        var result = await _repo.DeleteAsync(null!);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsWhitespace()
    {
        var result = await _repo.DeleteAsync("   ");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsInvalid()
    {
        var result = await _repo.DeleteAsync("invalid-id");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteExistingUser()
    {
        var user = new User
        {
            Email = "delete@test.com",
            PasswordHash = "x"
        };

        await _repo.CreateAsync(user);

        var deleted = await _repo.DeleteAsync(user.Id);

        deleted.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(user.Id);

        fetched.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        var result = await _repo.DeleteAsync(
            ObjectId.GenerateNewId().ToString());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldOnlyDeleteTargetUser()
    {
        var user1 = new User
        {
            Email = "delete1@test.com",
            PasswordHash = "x"
        };

        var user2 = new User
        {
            Email = "delete2@test.com",
            PasswordHash = "y"
        };

        await _repo.CreateAsync(user1);
        await _repo.CreateAsync(user2);

        var deleted = await _repo.DeleteAsync(user1.Id);

        deleted.Should().BeTrue();

        (await _repo.GetByIdAsync(user1.Id))
            .Should().BeNull();

        (await _repo.GetByIdAsync(user2.Id))
            .Should().NotBeNull();
    }

    // ---------------------------------------------------------
    // TEST HELPERS
    // ---------------------------------------------------------

    private static User CreateUser()
    {
        var createdAt = DateTime.UtcNow.AddMinutes(-10);
        var updatedAt = DateTime.UtcNow.AddMinutes(-5);

        return new User
        {
            Email = $"user-{Guid.NewGuid()}@test.com",
            PasswordHash = "test-password-hash",
            FullName = "Test User",
            Roles = new List<string> { "User" },

            PhoneNumber = "+46 70 123 45 67",

            ShippingAddress = new Address
            {
                Street = "Shipping Street 1",
                City = "Gothenburg",
                State = "Västra Götaland",
                PostalCode = "411 01",
                Country = "Sweden"
            },

            BillingAddress = new Address
            {
                Street = "Billing Street 2",
                City = "Stockholm",
                State = "Stockholm",
                PostalCode = "111 01",
                Country = "Sweden"
            },

            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            IsEmailVerified = true
        };
    }
}
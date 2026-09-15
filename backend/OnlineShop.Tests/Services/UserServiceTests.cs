using MongoDB.Bson;
using Moq;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using OnlineShop.Api.Services;

namespace OnlineShop.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _service = new UserService(_repoMock.Object);
    }

    // =========================================================
    // GetAllAsync
    // =========================================================

    [Fact]
    public async Task GetAllAsync_ShouldReturnUsers()
    {
        var users = new List<User>
        {
            CreateUser(),
            CreateUser()
        };

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(users, result);

        _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        var result = await _service.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // =========================================================
    // GetByIdAsync
    // =========================================================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        var user = CreateUser();

        _repoMock
            .Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        var result = await _service.GetByIdAsync(
            user.Id,
            user.Id,
            false);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result!.Id);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((User?)null);

        var result = await _service.GetByIdAsync(
            id,
            id,
            false);

        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-id")]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsInvalid(string? id)
    {
        var result = await _service.GetByIdAsync(
            id!,
            id!,
            false);

        Assert.Null(result);

        _repoMock.Verify(
            r => r.GetByIdAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNewProfileFields()
    {
        var user = CreateUser();

        user.PhoneNumber = "+46701234567";

        user.ShippingAddress = new Address
        {
            Street = "Main Street 10",
            City = "Gothenburg",
            State = "Vastra Gotaland",
            PostalCode = "41101",
            Country = "Sweden"
        };

        user.BillingAddress = new Address
        {
            Street = "Billing Street 20",
            City = "Stockholm",
            State = "Stockholm",
            PostalCode = "11101",
            Country = "Sweden"
        };

        user.IsEmailVerified = true;

        _repoMock
            .Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        var result = await _service.GetByIdAsync(
            user.Id,
            user.Id,
            false);

        Assert.NotNull(result);

        Assert.Equal("+46701234567", result!.PhoneNumber);

        Assert.NotNull(result.ShippingAddress);
        Assert.Equal(
            "Main Street 10",
            result.ShippingAddress!.Street);

        Assert.Equal(
            "Gothenburg",
            result.ShippingAddress.City);

        Assert.NotNull(result.BillingAddress);
        Assert.Equal(
            "Billing Street 20",
            result.BillingAddress!.Street);

        Assert.True(result.IsEmailVerified);
    }

    // =========================================================
    // CreateAsync
    // =========================================================

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenUserIsNull()
    {
        var result = await _service.CreateAsync(null!);

        Assert.Null(result);

        _repoMock.Verify(
            r => r.CreateAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_ShouldReturnNull_WhenEmailIsInvalid(
        string? email)
    {
        var user = CreateUser();
        user.Email = email!;

        var result = await _service.CreateAsync(user);

        Assert.Null(result);

        _repoMock.Verify(
            r => r.CreateAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_ShouldReturnNull_WhenPasswordHashIsInvalid(
        string? passwordHash)
    {
        var user = CreateUser();
        user.PasswordHash = passwordHash!;

        var result = await _service.CreateAsync(user);

        Assert.Null(result);

        _repoMock.Verify(
            r => r.CreateAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenEmailAlreadyExists()
    {
        var existingUser = CreateUser();
        existingUser.Email = "existing@example.com";

        var newUser = CreateUser();
        newUser.Email = "existing@example.com";

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        var result = await _service.CreateAsync(newUser);

        Assert.Null(result);

        _repoMock.Verify(
            r => r.CreateAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldDetectDuplicateEmailCaseInsensitive()
    {
        var existingUser = CreateUser();
        existingUser.Email = "user@example.com";

        var newUser = CreateUser();
        newUser.Email = "USER@EXAMPLE.COM";

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        var result = await _service.CreateAsync(newUser);

        Assert.Null(result);

        _repoMock.Verify(
            r => r.CreateAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimEmail()
    {
        var user = CreateUser();
        user.Email = "  user@example.com  ";

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(user);

        Assert.NotNull(result);
        Assert.Equal("user@example.com", result!.Email);

        _repoMock.Verify(
            r => r.CreateAsync(It.Is<User>(
                u => u.Email == "user@example.com")),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateId_WhenIdIsMissing()
    {
        var user = CreateUser();
        user.Id = "";

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(user);

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result!.Id));
        Assert.True(ObjectId.TryParse(result.Id, out _));

        _repoMock.Verify(
            r => r.CreateAsync(It.IsAny<User>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateId_WhenIdIsInvalid()
    {
        var user = CreateUser();
        user.Id = "invalid-id";

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(user);

        Assert.NotNull(result);
        Assert.True(ObjectId.TryParse(result!.Id, out _));

        _repoMock.Verify(
            r => r.CreateAsync(It.IsAny<User>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldSetCreatedAtAndUpdatedAtToSameValue()
    {
        var user = CreateUser();

        user.CreatedAt = default;
        user.UpdatedAt = default;

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(user);

        Assert.NotNull(result);

        Assert.NotEqual(default, result!.CreatedAt);
        Assert.NotEqual(default, result.UpdatedAt);
        Assert.Equal(result.CreatedAt, result.UpdatedAt);
    }

    [Fact]
    public async Task CreateAsync_ShouldSetEmailAsUnverified()
    {
        var user = CreateUser();

        user.IsEmailVerified = false;

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(user);

        Assert.NotNull(result);
        Assert.False(result!.IsEmailVerified);
    }

    [Fact]
    public async Task CreateAsync_ShouldPreserveEmailVerification_WhenExplicitlyVerified()
    {
        var user = CreateUser();

        user.IsEmailVerified = true;

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(user);

        Assert.NotNull(result);
        Assert.True(result!.IsEmailVerified);
    }

    [Fact]
    public async Task CreateAsync_ShouldSetDefaultUserRole_WhenRolesAreEmpty()
    {
        var user = CreateUser();

        user.Roles = new List<string>();

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(user);

        Assert.NotNull(result);

        Assert.Single(result!.Roles);
        Assert.Equal("User", result.Roles[0]);
    }

    [Fact]
    public async Task CreateAsync_ShouldPreserveExistingRoles()
    {
        var user = CreateUser();

        user.Roles = new List<string>
        {
            "Admin",
            "User"
        };

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(user);

        Assert.NotNull(result);

        Assert.Equal(2, result!.Roles.Count);
        Assert.Contains("Admin", result.Roles);
        Assert.Contains("User", result.Roles);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistPhoneAndAddresses()
    {
        var user = CreateUser();

        user.PhoneNumber = "+46701234567";

        user.ShippingAddress = new Address
        {
            Street = "Shipping Street 1",
            City = "Gothenburg",
            State = "Vastra Gotaland",
            PostalCode = "41101",
            Country = "Sweden"
        };

        user.BillingAddress = new Address
        {
            Street = "Billing Street 2",
            City = "Stockholm",
            State = "Stockholm",
            PostalCode = "11101",
            Country = "Sweden"
        };

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(user);

        Assert.NotNull(result);

        Assert.Equal("+46701234567", result!.PhoneNumber);

        Assert.NotNull(result.ShippingAddress);
        Assert.Equal(
            "Shipping Street 1",
            result.ShippingAddress!.Street);

        Assert.NotNull(result.BillingAddress);
        Assert.Equal(
            "Billing Street 2",
            result.BillingAddress!.Street);
    }

    // =========================================================
    // UpdateProfileAsync - Validation
    // =========================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-id")]
    public async Task UpdateProfileAsync_ShouldReturnNull_WhenIdIsInvalid(
        string? id)
    {
        var result = await _service.UpdateProfileAsync(
            id!,
            "Updated Name",
            "updated@example.com",
            null,
            null,
            null);

        Assert.Null(result);

        _repoMock.Verify(
            r => r.GetByIdAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateProfileAsync_ShouldReturnNull_WhenFullNameIsInvalid(
        string? fullName)
    {
        var id = ObjectId.GenerateNewId().ToString();

        var result = await _service.UpdateProfileAsync(
            id,
            fullName!,
            "updated@example.com",
            null,
            null,
            null);

        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateProfileAsync_ShouldReturnNull_WhenEmailIsInvalid(
        string? email)
    {
        var id = ObjectId.GenerateNewId().ToString();

        var result = await _service.UpdateProfileAsync(
            id,
            "Updated Name",
            email!,
            null,
            null,
            null);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((User?)null);

        var result = await _service.UpdateProfileAsync(
            id,
            "Updated Name",
            "updated@example.com",
            null,
            null,
            null);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldReturnNull_WhenEmailAlreadyExists()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var existingUser = CreateUser();
        existingUser.Id = id;
        existingUser.Email = "old@example.com";

        var otherUser = CreateUser();
        otherUser.Email = "taken@example.com";

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser,
                otherUser
            });

        var result = await _service.UpdateProfileAsync(
            id,
            "Updated Name",
            "taken@example.com",
            null,
            null,
            null);

        Assert.Null(result);

        _repoMock.Verify(
            r => r.UpdateAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldDetectDuplicateEmailCaseInsensitive()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var existingUser = CreateUser();
        existingUser.Id = id;
        existingUser.Email = "old@example.com";

        var otherUser = CreateUser();
        otherUser.Email = "taken@example.com";

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser,
                otherUser
            });

        var result = await _service.UpdateProfileAsync(
            id,
            "Updated Name",
            "TAKEN@EXAMPLE.COM",
            null,
            null,
            null);

        Assert.Null(result);

        _repoMock.Verify(
            r => r.UpdateAsync(It.IsAny<User>()),
            Times.Never);
    }

    // =========================================================
    // UpdateProfileAsync - Successful updates
    // =========================================================

    [Fact]
    public async Task UpdateProfileAsync_ShouldTrimFullNameAndEmail()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var existingUser = CreateUser();
        existingUser.Id = id;
        existingUser.Email = "old@example.com";

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateProfileAsync(
            id,
            "  Updated Name  ",
            "  updated@example.com  ",
            null,
            null,
            null);

        Assert.NotNull(result);

        Assert.Equal("Updated Name", result!.FullName);
        Assert.Equal("updated@example.com", result.Email);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldUpdateFullNameAndEmail()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var existingUser = CreateUser();
        existingUser.Id = id;
        existingUser.Email = "old@example.com";
        existingUser.FullName = "Old Name";

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateProfileAsync(
            id,
            "New Name",
            "new@example.com",
            null,
            null,
            null);

        Assert.NotNull(result);

        Assert.Equal("New Name", result!.FullName);
        Assert.Equal("new@example.com", result.Email);

        _repoMock.Verify(
            r => r.UpdateAsync(It.Is<User>(
                u =>
                    u.Id == id &&
                    u.FullName == "New Name" &&
                    u.Email == "new@example.com")),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldAllowSameEmail()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var existingUser = CreateUser();
        existingUser.Id = id;
        existingUser.Email = "user@example.com";

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateProfileAsync(
            id,
            "Updated Name",
            "user@example.com",
            null,
            null,
            null);

        Assert.NotNull(result);
        Assert.Equal("user@example.com", result!.Email);

        _repoMock.Verify(
            r => r.UpdateAsync(It.IsAny<User>()),
            Times.Once);
    }

    // =========================================================
    // UpdateProfileAsync - Phone
    // =========================================================

    [Fact]
    public async Task UpdateProfileAsync_ShouldUpdatePhoneNumber()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var existingUser = CreateUser();
        existingUser.Id = id;
        existingUser.PhoneNumber = "+46111111111";

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateProfileAsync(
            id,
            existingUser.FullName,
            existingUser.Email,
            "+46701234567",
            null,
            null);

        Assert.NotNull(result);
        Assert.Equal("+46701234567", result!.PhoneNumber);

        _repoMock.Verify(
            r => r.UpdateAsync(It.Is<User>(
                u => u.PhoneNumber == "+46701234567")),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldClearPhoneNumber_WhenWhitespace()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var existingUser = CreateUser();
        existingUser.Id = id;
        existingUser.PhoneNumber = "+46701234567";

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateProfileAsync(
            id,
            existingUser.FullName,
            existingUser.Email,
            "   ",
            null,
            null);

        Assert.NotNull(result);
        Assert.Null(result!.PhoneNumber);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldPreservePhoneNumber_WhenNotProvided()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var existingUser = CreateUser();
        existingUser.Id = id;
        existingUser.PhoneNumber = "+46701234567";

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateProfileAsync(
            id,
            existingUser.FullName,
            existingUser.Email,
            null,
            null,
            null);

        Assert.NotNull(result);
        Assert.Equal("+46701234567", result!.PhoneNumber);
    }

    // =========================================================
    // UpdateProfileAsync - Addresses
    // =========================================================

    [Fact]
    public async Task UpdateProfileAsync_ShouldUpdateShippingAddress()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var existingUser = CreateUser();
        existingUser.Id = id;

        var shippingAddress = new Address
        {
            Street = "New Shipping Street 10",
            City = "Gothenburg",
            State = "Vastra Gotaland",
            PostalCode = "41101",
            Country = "Sweden"
        };

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateProfileAsync(
            id,
            existingUser.FullName,
            existingUser.Email,
            null,
            shippingAddress,
            null);

        Assert.NotNull(result);
        Assert.NotNull(result!.ShippingAddress);

        Assert.Equal(
            "New Shipping Street 10",
            result.ShippingAddress!.Street);

        Assert.Equal(
            "Gothenburg",
            result.ShippingAddress.City);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldUpdateBillingAddress()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var existingUser = CreateUser();
        existingUser.Id = id;

        var billingAddress = new Address
        {
            Street = "New Billing Street 20",
            City = "Stockholm",
            State = "Stockholm",
            PostalCode = "11101",
            Country = "Sweden"
        };

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateProfileAsync(
            id,
            existingUser.FullName,
            existingUser.Email,
            null,
            null,
            billingAddress);

        Assert.NotNull(result);
        Assert.NotNull(result!.BillingAddress);

        Assert.Equal(
            "New Billing Street 20",
            result.BillingAddress!.Street);

        Assert.Equal(
            "Stockholm",
            result.BillingAddress.City);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldUpdateBothAddresses()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var existingUser = CreateUser();
        existingUser.Id = id;

        var shippingAddress = new Address
        {
            Street = "Shipping Street",
            City = "Gothenburg",
            State = "Vastra Gotaland",
            PostalCode = "41101",
            Country = "Sweden"
        };

        var billingAddress = new Address
        {
            Street = "Billing Street",
            City = "Stockholm",
            State = "Stockholm",
            PostalCode = "11101",
            Country = "Sweden"
        };

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateProfileAsync(
            id,
            existingUser.FullName,
            existingUser.Email,
            null,
            shippingAddress,
            billingAddress);

        Assert.NotNull(result);

        Assert.NotNull(result!.ShippingAddress);
        Assert.Equal(
            "Shipping Street",
            result.ShippingAddress!.Street);

        Assert.NotNull(result.BillingAddress);
        Assert.Equal(
            "Billing Street",
            result.BillingAddress!.Street);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldPreserveAddresses_WhenNotProvided()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var existingUser = CreateUser();
        existingUser.Id = id;

        existingUser.ShippingAddress = new Address
        {
            Street = "Existing Shipping Street",
            City = "Gothenburg",
            State = "Vastra Gotaland",
            PostalCode = "41101",
            Country = "Sweden"
        };

        existingUser.BillingAddress = new Address
        {
            Street = "Existing Billing Street",
            City = "Stockholm",
            State = "Stockholm",
            PostalCode = "11101",
            Country = "Sweden"
        };

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateProfileAsync(
            id,
            existingUser.FullName,
            existingUser.Email,
            null,
            null,
            null);

        Assert.NotNull(result);

        Assert.NotNull(result!.ShippingAddress);
        Assert.Equal(
            "Existing Shipping Street",
            result.ShippingAddress!.Street);

        Assert.NotNull(result.BillingAddress);
        Assert.Equal(
            "Existing Billing Street",
            result.BillingAddress!.Street);
    }

    // =========================================================
    // UpdateProfileAsync - Timestamps
    // =========================================================

    [Fact]
    public async Task UpdateProfileAsync_ShouldUpdateUpdatedAt()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var oldUpdatedAt = DateTime.UtcNow.AddDays(-5);

        var existingUser = CreateUser();
        existingUser.Id = id;
        existingUser.UpdatedAt = oldUpdatedAt;

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateProfileAsync(
            id,
            "Updated Name",
            existingUser.Email,
            null,
            null,
            null);

        Assert.NotNull(result);

        Assert.True(result!.UpdatedAt > oldUpdatedAt);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldPreserveCreatedAt()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var createdAt = DateTime.UtcNow.AddDays(-10);

        var existingUser = CreateUser();
        existingUser.Id = id;
        existingUser.CreatedAt = createdAt;

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateProfileAsync(
            id,
            "Updated Name",
            existingUser.Email,
            null,
            null,
            null);

        Assert.NotNull(result);

        Assert.Equal(createdAt, result!.CreatedAt);
    }

    // =========================================================
    // UpdateProfileAsync - Email verification
    // =========================================================

    [Fact]
    public async Task UpdateProfileAsync_ShouldPreserveEmailVerificationStatus()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var existingUser = CreateUser();
        existingUser.Id = id;
        existingUser.Email = "verified@example.com";
        existingUser.IsEmailVerified = true;

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateProfileAsync(
            id,
            "Updated Name",
            "verified@example.com",
            null,
            null,
            null);

        Assert.NotNull(result);

        Assert.True(result!.IsEmailVerified);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldResetEmailVerification_WhenEmailChanges()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var existingUser = CreateUser();
        existingUser.Id = id;
        existingUser.Email = "old@example.com";
        existingUser.IsEmailVerified = true;

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateProfileAsync(
            id,
            "Updated Name",
            "new@example.com",
            null,
            null,
            null);

        Assert.NotNull(result);

        Assert.Equal("new@example.com", result!.Email);
        Assert.False(result.IsEmailVerified);
    }

    // =========================================================
    // UpdateProfileAsync - Password
    // =========================================================

    [Fact]
    public async Task UpdateProfileAsync_ShouldPreservePasswordHash()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var existingUser = CreateUser();
        existingUser.Id = id;
        existingUser.PasswordHash = "original-password-hash";

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateProfileAsync(
            id,
            "Updated Name",
            existingUser.Email,
            null,
            null,
            null);

        Assert.NotNull(result);

        Assert.Equal(
            "original-password-hash",
            result!.PasswordHash);
    }

    // =========================================================
    // UpdateProfileAsync - Repository failure
    // =========================================================

    [Fact]
    public async Task UpdateProfileAsync_ShouldReturnNull_WhenRepositoryUpdateFails()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var existingUser = CreateUser();
        existingUser.Id = id;

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingUser);

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>
            {
                existingUser
            });

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(false);

        var result = await _service.UpdateProfileAsync(
            id,
            "Updated Name",
            existingUser.Email,
            null,
            null,
            null);

        Assert.Null(result);
    }

    // =========================================================
    // AddRoleAsync
    // =========================================================

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserIdIsInvalid()
    {
        var result = await _service.AddRoleAsync(
            "invalid-id",
            "Admin");

        Assert.False(result);

        _repoMock.Verify(
            r => r.AddRoleAsync(
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenRoleIsInvalid(
        string? role)
    {
        var id = ObjectId.GenerateNewId().ToString();

        var result = await _service.AddRoleAsync(
            id,
            role!);

        Assert.False(result);

        _repoMock.Verify(
            r => r.AddRoleAsync(
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((User?)null);

        var result = await _service.AddRoleAsync(
            id,
            "Admin");

        Assert.False(result);
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnTrue_WhenRoleAlreadyExists()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var user = CreateUser();
        user.Id = id;
        user.Roles = new List<string>
        {
            "User",
            "Admin"
        };

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(user);

        var result = await _service.AddRoleAsync(
            id,
            "Admin");

        Assert.True(result);

        _repoMock.Verify(
            r => r.AddRoleAsync(
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task AddRoleAsync_ShouldAddRole()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var user = CreateUser();
        user.Id = id;
        user.Roles = new List<string>
        {
            "User"
        };

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(user);

        _repoMock
            .Setup(r => r.AddRoleAsync(id, "Admin"))
            .ReturnsAsync(true);

        var result = await _service.AddRoleAsync(
            id,
            "Admin");

        Assert.True(result);

        _repoMock.Verify(
            r => r.AddRoleAsync(id, "Admin"),
            Times.Once);
    }

    [Fact]
    public async Task AddRoleAsync_ShouldTrimRole()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var user = CreateUser();
        user.Id = id;
        user.Roles = new List<string>
        {
            "User"
        };

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(user);

        _repoMock
            .Setup(r => r.AddRoleAsync(id, "Admin"))
            .ReturnsAsync(true);

        var result = await _service.AddRoleAsync(
            id,
            "   Admin   ");

        Assert.True(result);

        _repoMock.Verify(
            r => r.AddRoleAsync(id, "Admin"),
            Times.Once);
    }

    // =========================================================
    // DeleteAsync
    // =========================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-id")]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsInvalid(
        string? id)
    {
        var result = await _service.DeleteAsync(id!);

        Assert.False(result);

        _repoMock.Verify(
            r => r.DeleteAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((User?)null);

        var result = await _service.DeleteAsync(id);

        Assert.False(result);

        _repoMock.Verify(
            r => r.DeleteAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteUser()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var user = CreateUser();
        user.Id = id;

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(user);

        _repoMock
            .Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(true);

        var result = await _service.DeleteAsync(id);

        Assert.True(result);

        _repoMock.Verify(
            r => r.DeleteAsync(id),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenRepositoryDeleteFails()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var user = CreateUser();
        user.Id = id;

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(user);

        _repoMock
            .Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(false);

        var result = await _service.DeleteAsync(id);

        Assert.False(result);

        _repoMock.Verify(
            r => r.DeleteAsync(id),
            Times.Once);
    }

    // =========================================================
    // Helpers
    // =========================================================

    private static User CreateUser()
    {
        var now = DateTime.UtcNow;

        return new User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = "user@example.com",
            PasswordHash = "hashed-password",
            FullName = "Test User",
            Roles = new List<string>
            {
                "User"
            },

            PhoneNumber = "+46701234567",

            ShippingAddress = new Address
            {
                Street = "Shipping Street 1",
                City = "Gothenburg",
                State = "Vastra Gotaland",
                PostalCode = "41101",
                Country = "Sweden"
            },

            BillingAddress = new Address
            {
                Street = "Billing Street 2",
                City = "Stockholm",
                State = "Stockholm",
                PostalCode = "11101",
                Country = "Sweden"
            },

            CreatedAt = now,
            UpdatedAt = now,

            IsEmailVerified = true
        };
    }
}
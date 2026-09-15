using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using OnlineShop.Api.Controllers;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;
using Xunit;
using System.Security.Claims;
using OnlineShop.Api.DTOs;

public class UsersControllerTests
{
    private UsersController CreateController(
        IUserService service,
        string? userId = null,
        bool isAdmin = false)
    {
        var controller = new UsersController(service);

        var httpContext = new DefaultHttpContext();

        var claims = new List<Claim>();

        if (userId != null)
            claims.Add(new Claim("sub", userId));

        if (isAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(claims, "test")
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenAdmin()
    {
        var service = new FakeUserService();

        var controller = CreateController(
            service,
            ObjectId.GenerateNewId().ToString(),
            isAdmin: true
        );

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnForbid_WhenNotAdmin()
    {
        var controller = CreateController(
            new FakeUserService(),
            ObjectId.GenerateNewId().ToString(),
            isAdmin: false
        );

        var result = await controller.GetAll();

        result.Should().BeOfType<ForbidResult>();
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetById_ShouldReturnUnauthorized_WhenUserIdInvalid()
    {
        var controller = CreateController(
            new FakeUserService(),
            "invalid-user"
        );

        var result = await controller.GetById(
            ObjectId.GenerateNewId().ToString()
        );

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnUnauthorized_WhenUserIdMissing()
    {
        var controller = CreateController(
            new FakeUserService()
        );

        var result = await controller.GetById(
            ObjectId.GenerateNewId().ToString()
        );

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnBadRequest_WhenIdInvalid()
    {
        var controller = CreateController(
            new FakeUserService(),
            ObjectId.GenerateNewId().ToString()
        );

        var result = await controller.GetById("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnBadRequest_WhenIdWhitespace()
    {
        var controller = CreateController(
            new FakeUserService(),
            ObjectId.GenerateNewId().ToString()
        );

        var result = await controller.GetById("   ");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnForbid_WhenNotOwnerAndNotAdmin()
    {
        var service = new FakeUserService();

        var ownerId = ObjectId.GenerateNewId().ToString();
        var otherId = ObjectId.GenerateNewId().ToString();

        service.AddUser(ownerId);

        var controller = CreateController(
            service,
            otherId
        );

        var result = await controller.GetById(ownerId);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenOwner()
    {
        var service = new FakeUserService();

        var userId = ObjectId.GenerateNewId().ToString();

        service.AddUser(userId);

        var controller = CreateController(
            service,
            userId
        );

        var result = await controller.GetById(userId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenAdmin()
    {
        var service = new FakeUserService();

        var userId = ObjectId.GenerateNewId().ToString();
        var adminId = ObjectId.GenerateNewId().ToString();

        service.AddUser(userId);
        service.AddUser(adminId);

        var controller = CreateController(
            service,
            adminId,
            isAdmin: true
        );

        var result = await controller.GetById(userId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenUserMissing()
    {
        var controller = CreateController(
            new FakeUserService(),
            ObjectId.GenerateNewId().ToString()
        );

        var result = await controller.GetById(
            ObjectId.GenerateNewId().ToString()
        );

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ---------------------------------------------------------
    // ADD ROLE
    // ---------------------------------------------------------

    [Fact]
    public async Task AddRole_ShouldReturnUnauthorized_WhenNotAdmin()
    {
        var controller = CreateController(
            new FakeUserService(),
            ObjectId.GenerateNewId().ToString(),
            isAdmin: false
        );

        var result = await controller.AddRole(
            ObjectId.GenerateNewId().ToString(),
            "Admin"
        );

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task AddRole_ShouldReturnBadRequest_WhenIdInvalid()
    {
        var controller = CreateController(
            new FakeUserService(),
            ObjectId.GenerateNewId().ToString(),
            isAdmin: true
        );

        var result = await controller.AddRole(
            "invalid-id",
            "Admin"
        );

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddRole_ShouldReturnBadRequest_WhenRoleMissing()
    {
        var controller = CreateController(
            new FakeUserService(),
            ObjectId.GenerateNewId().ToString(),
            isAdmin: true
        );

        var id = ObjectId.GenerateNewId().ToString();

        var result = await controller.AddRole(id, "");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddRole_ShouldReturnBadRequest_WhenRoleWhitespace()
    {
        var controller = CreateController(
            new FakeUserService(),
            ObjectId.GenerateNewId().ToString(),
            isAdmin: true
        );

        var id = ObjectId.GenerateNewId().ToString();

        var result = await controller.AddRole(id, "   ");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddRole_ShouldReturnBadRequest_WhenRoleNull()
    {
        var controller = CreateController(
            new FakeUserService(),
            ObjectId.GenerateNewId().ToString(),
            isAdmin: true
        );

        var id = ObjectId.GenerateNewId().ToString();

        var result = await controller.AddRole(id, null);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddRole_ShouldReturnNotFound_WhenUserMissing()
    {
        var controller = CreateController(
            new FakeUserService(),
            ObjectId.GenerateNewId().ToString(),
            isAdmin: true
        );

        var id = ObjectId.GenerateNewId().ToString();

        var result = await controller.AddRole(id, "Admin");

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AddRole_ShouldReturnOk_WhenValid()
    {
        var service = new FakeUserService();

        var id = ObjectId.GenerateNewId().ToString();

        service.AddUser(id);

        var controller = CreateController(
            service,
            ObjectId.GenerateNewId().ToString(),
            isAdmin: true
        );

        var result = await controller.AddRole(id, "Admin");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AddRole_ShouldReturnOk_WhenRoleAlreadyExists()
    {
        var service = new FakeUserService();

        var id = ObjectId.GenerateNewId().ToString();

        service.AddUser(id);
        await service.AddRoleAsync(id, "Admin");

        var controller = CreateController(
            service,
            ObjectId.GenerateNewId().ToString(),
            isAdmin: true
        );

        var result = await controller.AddRole(id, "Admin");

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // UPDATE PROFILE
    // ---------------------------------------------------------

    [Fact]
    public async Task Update_ShouldReturnUnauthorized_WhenUserIdInvalid()
    {
        var controller = CreateController(
            new FakeUserService(),
            "invalid-user"
        );

        var dto = new UpdateProfileDto
        {
            FullName = "Updated User",
            Email = "updated@example.com"
        };

        var result = await controller.Update(
            ObjectId.GenerateNewId().ToString(),
            dto
        );

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnUnauthorized_WhenUserIdMissing()
    {
        var controller = CreateController(
            new FakeUserService()
        );

        var dto = new UpdateProfileDto
        {
            FullName = "Updated User",
            Email = "updated@example.com"
        };

        var result = await controller.Update(
            ObjectId.GenerateNewId().ToString(),
            dto
        );

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenIdInvalid()
    {
        var controller = CreateController(
            new FakeUserService(),
            ObjectId.GenerateNewId().ToString()
        );

        var dto = new UpdateProfileDto
        {
            FullName = "Updated User",
            Email = "updated@example.com"
        };

        var result = await controller.Update(
            "invalid-id",
            dto
        );

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenDtoNull()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var controller = CreateController(
            new FakeUserService(),
            userId
        );

        var result = await controller.Update(
            userId,
            null
        );

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnForbid_WhenNotOwnerAndNotAdmin()
    {
        var service = new FakeUserService();

        var ownerId = ObjectId.GenerateNewId().ToString();
        var otherId = ObjectId.GenerateNewId().ToString();

        service.AddUser(ownerId);

        var controller = CreateController(
            service,
            otherId
        );

        var dto = new UpdateProfileDto
        {
            FullName = "Updated User",
            Email = "updated@example.com"
        };

        var result = await controller.Update(
            ownerId,
            dto
        );

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenFullNameMissing()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var controller = CreateController(
            new FakeUserService(),
            userId
        );

        var dto = new UpdateProfileDto
        {
            FullName = "",
            Email = "updated@example.com"
        };

        var result = await controller.Update(
            userId,
            dto
        );

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenFullNameWhitespace()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var controller = CreateController(
            new FakeUserService(),
            userId
        );

        var dto = new UpdateProfileDto
        {
            FullName = "   ",
            Email = "updated@example.com"
        };

        var result = await controller.Update(
            userId,
            dto
        );

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenEmailMissing()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var controller = CreateController(
            new FakeUserService(),
            userId
        );

        var dto = new UpdateProfileDto
        {
            FullName = "Updated User",
            Email = ""
        };

        var result = await controller.Update(
            userId,
            dto
        );

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenEmailWhitespace()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var controller = CreateController(
            new FakeUserService(),
            userId
        );

        var dto = new UpdateProfileDto
        {
            FullName = "Updated User",
            Email = "   "
        };

        var result = await controller.Update(
            userId,
            dto
        );

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenEmailInvalid()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var controller = CreateController(
            new FakeUserService(),
            userId
        );

        var dto = new UpdateProfileDto
        {
            FullName = "Updated User",
            Email = "invalid-email"
        };

        var result = await controller.Update(
            userId,
            dto
        );

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenUserMissing()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var controller = CreateController(
            new FakeUserService(),
            userId
        );

        var dto = new UpdateProfileDto
        {
            FullName = "Updated User",
            Email = "updated@example.com"
        };

        var result = await controller.Update(
            userId,
            dto
        );

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenOwnerUpdatesProfile()
    {
        var service = new FakeUserService();

        var userId = ObjectId.GenerateNewId().ToString();

        service.AddUser(userId);

        var controller = CreateController(
            service,
            userId
        );

        var dto = new UpdateProfileDto
        {
            FullName = "Updated User",
            Email = "updated@example.com"
        };

        var result = await controller.Update(
            userId,
            dto
        );

        result.Should().BeOfType<OkObjectResult>();

        var user = service.GetStoredUser(userId);

        user.Should().NotBeNull();
        user!.FullName.Should().Be("Updated User");
        user.Email.Should().Be("updated@example.com");
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenAdminUpdatesAnotherUser()
    {
        var service = new FakeUserService();

        var userId = ObjectId.GenerateNewId().ToString();
        var adminId = ObjectId.GenerateNewId().ToString();

        service.AddUser(userId);
        service.AddUser(adminId);

        var controller = CreateController(
            service,
            adminId,
            isAdmin: true
        );

        var dto = new UpdateProfileDto
        {
            FullName = "Admin Updated",
            Email = "admin-updated@example.com"
        };

        var result = await controller.Update(
            userId,
            dto
        );

        result.Should().BeOfType<OkObjectResult>();

        var user = service.GetStoredUser(userId);

        user.Should().NotBeNull();
        user!.FullName.Should().Be("Admin Updated");
        user.Email.Should().Be("admin-updated@example.com");
    }

    [Fact]
    public async Task Update_ShouldTrimFullNameAndEmail()
    {
        var service = new FakeUserService();

        var userId = ObjectId.GenerateNewId().ToString();

        service.AddUser(userId);

        var controller = CreateController(
            service,
            userId
        );

        var dto = new UpdateProfileDto
        {
            FullName = "  Updated User  ",
            Email = "  updated@example.com  "
        };

        var result = await controller.Update(
            userId,
            dto
        );

        result.Should().BeOfType<OkObjectResult>();

        var user = service.GetStoredUser(userId);

        user.Should().NotBeNull();
        user!.FullName.Should().Be("Updated User");
        user.Email.Should().Be("updated@example.com");
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task Delete_ShouldReturnUnauthorized_WhenNotAdmin()
    {
        var controller = CreateController(
            new FakeUserService(),
            ObjectId.GenerateNewId().ToString(),
            isAdmin: false
        );

        var result = await controller.Delete(
            ObjectId.GenerateNewId().ToString()
        );

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenIdInvalid()
    {
        var controller = CreateController(
            new FakeUserService(),
            ObjectId.GenerateNewId().ToString(),
            isAdmin: true
        );

        var result = await controller.Delete("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(
            new FakeUserService(),
            ObjectId.GenerateNewId().ToString(),
            isAdmin: true
        );

        var id = ObjectId.GenerateNewId().ToString();

        var result = await controller.Delete(id);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenValid()
    {
        var service = new FakeUserService();

        var id = ObjectId.GenerateNewId().ToString();

        service.AddUser(id);

        var controller = CreateController(
            service,
            ObjectId.GenerateNewId().ToString(),
            isAdmin: true
        );

        var result = await controller.Delete(id);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenDeletingTwice()
    {
        var service = new FakeUserService();

        var id = ObjectId.GenerateNewId().ToString();

        service.AddUser(id);

        var controller = CreateController(
            service,
            ObjectId.GenerateNewId().ToString(),
            isAdmin: true
        );

        var first = await controller.Delete(id);
        var second = await controller.Delete(id);

        first.Should().BeOfType<OkObjectResult>();
        second.Should().BeOfType<NotFoundObjectResult>();
    }
}

// ---------------------------------------------------------
// FAKE SERVICE FOR TESTING
// ---------------------------------------------------------

public class FakeUserService : IUserService
{
    private readonly Dictionary<string, User> _store = new();

    public void AddUser(string id)
    {
        _store[id] = new User
        {
            Id = id,
            Email = "test@example.com",
            PasswordHash = "hashed-password",
            FullName = "Test User",
            Roles = new List<string> { "User" },

            // New profile fields
            PhoneNumber = "+46 70 000 00 00",

            ShippingAddress = new Address
            {
                Street = "Test Street 1",
                City = "Gothenburg",
                State = "Vastra Gotaland",
                PostalCode = "411 01",
                Country = "Sweden"
            },

            BillingAddress = new Address
            {
                Street = "Billing Street 1",
                City = "Gothenburg",
                State = "Vastra Gotaland",
                PostalCode = "411 01",
                Country = "Sweden"
            },

            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow,
            IsEmailVerified = true
        };
    }

    public User? GetStoredUser(string id)
    {
        _store.TryGetValue(id, out var user);
        return user;
    }

    public Task<User?> CreateAsync(User user)
    {
        _store[user.Id] = user;

        return Task.FromResult<User?>(user);
    }

    public Task<List<User>> GetAllAsync()
        => Task.FromResult(_store.Values.ToList());

    public Task<User?> GetByIdAsync(
        string id,
        string currentUserId,
        bool isAdmin)
    {
        if (!_store.ContainsKey(id))
            return Task.FromResult<User?>(null);

        var user = _store[id];

        if (isAdmin || id == currentUserId)
            return Task.FromResult<User?>(user);

        return Task.FromResult<User?>(null);
    }

    public Task<User?> UpdateProfileAsync(
    string id,
    string fullName,
    string email,
    string? phoneNumber,
    Address? shippingAddress,
    Address? billingAddress)
    {
        if (!_store.TryGetValue(id, out var user))
            return Task.FromResult<User?>(null);

        var emailChanged = !string.Equals(
            user.Email,
            email,
            StringComparison.OrdinalIgnoreCase
        );

        user.FullName = fullName;
        user.Email = email;
        user.PhoneNumber = phoneNumber;
        user.ShippingAddress = shippingAddress;
        user.BillingAddress = billingAddress;
        user.UpdatedAt = DateTime.UtcNow;

        // Changing the email requires verification again.
        if (emailChanged)
            user.IsEmailVerified = false;

        return Task.FromResult<User?>(user);
    }

    public Task<bool> AddRoleAsync(string id, string role)
    {
        if (!_store.ContainsKey(id))
            return Task.FromResult(false);

        if (!_store[id].Roles.Contains(role))
            _store[id].Roles.Add(role);

        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(string id)
        => Task.FromResult(_store.Remove(id));
}

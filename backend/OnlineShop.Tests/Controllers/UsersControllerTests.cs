using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Controllers;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;
using Xunit;
using MongoDB.Bson;
using System.Security.Claims;

public class UsersControllerTests
{
    private UsersController CreateController(IUserService service, string? userId = null, bool isAdmin = false)
    {
        var controller = new UsersController(service);

        var httpContext = new DefaultHttpContext();

        var claims = new List<Claim>();

        if (userId != null)
            claims.Add(new Claim("sub", userId));

        if (isAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

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
        var controller = CreateController(new FakeUserService(), ObjectId.GenerateNewId().ToString(), isAdmin: true);

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetById_ShouldReturnUnauthorized_WhenUserIdInvalid()
    {
        var controller = CreateController(new FakeUserService(), "invalid-user");

        var result = await controller.GetById(ObjectId.GenerateNewId().ToString());

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnBadRequest_WhenIdInvalid()
    {
        var controller = CreateController(new FakeUserService(), ObjectId.GenerateNewId().ToString());

        var result = await controller.GetById("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnForbid_WhenNotOwnerAndNotAdmin()
    {
        var service = new FakeUserService();
        var ownerId = ObjectId.GenerateNewId().ToString();
        var otherId = ObjectId.GenerateNewId().ToString();

        service.AddUser(ownerId);

        var controller = CreateController(service, otherId);

        var result = await controller.GetById(ownerId);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenOwner()
    {
        var service = new FakeUserService();
        var userId = ObjectId.GenerateNewId().ToString();

        service.AddUser(userId);

        var controller = CreateController(service, userId);

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

        var controller = CreateController(service, adminId, isAdmin: true);

        var result = await controller.GetById(userId);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // ADD ROLE
    // ---------------------------------------------------------

    [Fact]
    public async Task AddRole_ShouldReturnBadRequest_WhenIdInvalid()
    {
        var controller = CreateController(new FakeUserService(), ObjectId.GenerateNewId().ToString(), isAdmin: true);

        var result = await controller.AddRole("invalid-id", "Admin");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddRole_ShouldReturnBadRequest_WhenRoleMissing()
    {
        var controller = CreateController(new FakeUserService(), ObjectId.GenerateNewId().ToString(), isAdmin: true);

        var id = ObjectId.GenerateNewId().ToString();
        var result = await controller.AddRole(id, "");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddRole_ShouldReturnNotFound_WhenUserMissing()
    {
        var controller = CreateController(new FakeUserService(), ObjectId.GenerateNewId().ToString(), isAdmin: true);

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

        var controller = CreateController(service, ObjectId.GenerateNewId().ToString(), isAdmin: true);

        var result = await controller.AddRole(id, "Admin");

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenIdInvalid()
    {
        var controller = CreateController(new FakeUserService(), ObjectId.GenerateNewId().ToString(), isAdmin: true);

        var result = await controller.Delete("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(new FakeUserService(), ObjectId.GenerateNewId().ToString(), isAdmin: true);

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

        var controller = CreateController(service, ObjectId.GenerateNewId().ToString(), isAdmin: true);

        var result = await controller.Delete(id);

        result.Should().BeOfType<OkObjectResult>();
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
            FullName = "Test User",
            Roles = new List<string>()
        };
    }

    public Task<List<User>> GetAllAsync()
        => Task.FromResult(_store.Values.ToList());

    public Task<User?> GetByIdAsync(string id, string currentUserId, bool isAdmin)
    {
        if (!_store.ContainsKey(id))
            return Task.FromResult<User?>(null);

        var user = _store[id];

        if (isAdmin || id == currentUserId)
            return Task.FromResult<User?>(user);

        return Task.FromResult<User?>(null);
    }

    public Task<bool> AddRoleAsync(string id, string role)
    {
        if (!_store.ContainsKey(id))
            return Task.FromResult(false);

        _store[id].Roles.Add(role);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(string id)
        => Task.FromResult(_store.Remove(id));
}

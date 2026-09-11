using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using OnlineShop.Api.Services;
using Xunit;

public class UserServiceTests : RepositoryTestBase
{
    private readonly UserService _service;
    private readonly UserRepository _repo;
    private readonly IMongoCollection<User> _users;

    public UserServiceTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new UserRepository(config);
        _service = new UserService(_repo);

        _users = Fixture.Database.GetCollection<User>("Users");
        Fixture.Database.DropCollection("Users");
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoUsers()
    {
        var result = await _service.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        await _repo.CreateAsync(new User { Id = ObjectId.GenerateNewId().ToString(), Email = "a@test.com" });
        await _repo.CreateAsync(new User { Id = ObjectId.GenerateNewId().ToString(), Email = "b@test.com" });

        var result = await _service.GetAllAsync();
        result.Should().HaveCount(2);
    }

    // ---------------------------------------------------------
    // GET BY ID — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdNull()
    {
        var result = await _service.GetByIdAsync(null!, "ignored", true);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdWhitespace()
    {
        var result = await _service.GetByIdAsync("   ", "ignored", true);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdInvalid()
    {
        var result = await _service.GetByIdAsync("invalid-id", "ignored", true);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_ForAdmin()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var user = new User { Id = id, Email = "admin@test.com" };
        await _repo.CreateAsync(user);

        var result = await _service.GetByIdAsync(id, "ignored", true);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenCurrentUserMatches()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var user = new User { Id = id, Email = "me@test.com" };
        await _repo.CreateAsync(user);

        var result = await _service.GetByIdAsync(id, id, false);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUnauthorized()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var otherId = ObjectId.GenerateNewId().ToString();

        var user = new User { Id = id, Email = "me@test.com" };
        await _repo.CreateAsync(user);

        var result = await _service.GetByIdAsync(id, otherId, false);
        result.Should().BeNull();
    }

    // ---------------------------------------------------------
    // CREATE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenUserIsNull()
    {
        var result = await _service.CreateAsync(null!);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenEmailNull()
    {
        var result = await _service.CreateAsync(new User { Email = null! });
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenEmailWhitespace()
    {
        var result = await _service.CreateAsync(new User { Email = "   " });
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenEmailAlreadyExists()
    {
        await _repo.CreateAsync(new User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = "dup@test.com"
        });

        var result = await _service.CreateAsync(new User
        {
            Email = "dup@test.com"
        });

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimEmail()
    {
        var created = await _service.CreateAsync(new User
        {
            Email = "   new@test.com   "
        });

        created.Should().NotBeNull();
        created!.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateId_WhenMissing()
    {
        var user = new User
        {
            Email = "new@test.com"
        };

        var created = await _service.CreateAsync(user);

        created.Should().NotBeNull();
        created!.Id.Should().NotBeNull();
    }

    // ---------------------------------------------------------
    // ADD ROLE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserIdNull()
    {
        var result = await _service.AddRoleAsync(null!, "Admin");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserIdWhitespace()
    {
        var result = await _service.AddRoleAsync("   ", "Admin");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserIdInvalid()
    {
        var result = await _service.AddRoleAsync("invalid-id", "Admin");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenRoleNull()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.AddRoleAsync(id, null!);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenRoleWhitespace()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.AddRoleAsync(id, "   ");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRoleAsync_ShouldAddRole_WhenValid()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var user = new User { Id = id, Email = "role@test.com", Roles = new List<string>() };
        await _repo.CreateAsync(user);

        var added = await _service.AddRoleAsync(id, "Admin");
        added.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(id);
        fetched.Should().NotBeNull();
        fetched!.Roles.Should().Contain("Admin");
    }

    // ---------------------------------------------------------
    // DELETE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdNull()
    {
        var result = await _service.DeleteAsync(null!);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdWhitespace()
    {
        var result = await _service.DeleteAsync("   ");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdInvalid()
    {
        var result = await _service.DeleteAsync("invalid-id");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenUserNotFound()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.DeleteAsync(id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDelete_WhenExists()
    {
        var id = ObjectId.GenerateNewId().ToString();

        var user = new User { Id = id, Email = "delete@test.com" };
        await _repo.CreateAsync(user);

        var deleted = await _service.DeleteAsync(id);
        deleted.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(id);
        fetched.Should().BeNull();
    }
}

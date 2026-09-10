using FluentAssertions;
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
        await _repo.CreateAsync(new User { Email = "a@test.com", PasswordHash = "x" });
        await _repo.CreateAsync(new User { Email = "b@test.com", PasswordHash = "y" });

        var result = await _repo.GetAllAsync();
        result.Should().HaveCount(2);
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
    {
        var user = new User { Email = "test@test.com", PasswordHash = "x" };
        await _repo.CreateAsync(user);

        var result = await _repo.GetByIdAsync(user.Id);
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repo.GetByIdAsync("missing-id");
        result.Should().BeNull();
    }

    // ---------------------------------------------------------
    // GET BY EMAIL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenExists()
    {
        var user = new User { Email = "email@test.com", PasswordHash = "x" };
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
    // ADD ROLE
    // ---------------------------------------------------------

    [Fact]
    public async Task AddRoleAsync_ShouldAddRoleToUser()
    {
        var user = new User { Email = "role@test.com", PasswordHash = "x", Roles = new List<string>() };
        await _repo.CreateAsync(user);

        var updated = await _repo.AddRoleAsync(user.Id, "Admin");
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

        var updated = await _repo.AddRoleAsync(user.Id, "Admin");
        updated.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(user.Id);
        fetched!.Roles.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        var result = await _repo.AddRoleAsync("missing-id", "Admin");
        result.Should().BeFalse();
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ShouldDeleteExistingUser()
    {
        var user = new User { Email = "delete@test.com", PasswordHash = "x" };
        await _repo.CreateAsync(user);

        var deleted = await _repo.DeleteAsync(user.Id);
        deleted.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(user.Id);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        var result = await _repo.DeleteAsync("missing-id");
        result.Should().BeFalse();
    }
}

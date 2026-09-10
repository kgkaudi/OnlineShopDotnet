using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using Xunit;

public class OrderRepositoryTests : RepositoryTestBase
{
    private readonly OrderRepository _repo;
    private readonly IMongoCollection<Order> _orders;

    public OrderRepositoryTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new OrderRepository(config);
        _orders = Fixture.Database.GetCollection<Order>("Orders");

        Fixture.Database.DropCollection("Orders");
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoOrders()
    {
        var result = await _repo.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllOrders()
    {
        await _repo.CreateAsync(new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 10
        });

        await _repo.CreateAsync(new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 20
        });

        var result = await _repo.GetAllAsync();
        result.Should().HaveCount(2);
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsInvalid()
    {
        var result = await _repo.GetByIdAsync("invalid-id");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _repo.GetByIdAsync(id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenExists()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 50
        };

        await _repo.CreateAsync(order);

        var result = await _repo.GetByIdAsync(order.Id);
        result.Should().NotBeNull();
        result!.Total.Should().Be(50);
    }

    // ---------------------------------------------------------
    // GET BY USER ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserIdIsInvalid()
    {
        var result = await _repo.GetByUserIdAsync("invalid-id");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserHasNoOrders()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var result = await _repo.GetByUserIdAsync(userId);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOrders_WhenUserHasOrders()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        await _repo.CreateAsync(new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            Total = 10
        });

        await _repo.CreateAsync(new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            Total = 20
        });

        var result = await _repo.GetByUserIdAsync(userId);
        result.Should().HaveCount(2);
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldInsertOrder()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 99
        };

        await _repo.CreateAsync(order);

        var fetched = await _repo.GetByIdAsync(order.Id);
        fetched.Should().NotBeNull();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsInvalid()
    {
        var order = new Order
        {
            Id = "invalid-id",
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 10
        };

        var result = await _repo.UpdateAsync(order);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenOrderDoesNotExist()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 10
        };

        var result = await _repo.UpdateAsync(order);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateOrder_WhenExists()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 10
        };

        await _repo.CreateAsync(order);

        order.Total = 200;

        var updated = await _repo.UpdateAsync(order);
        updated.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(order.Id);
        fetched!.Total.Should().Be(200);
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsInvalid()
    {
        var result = await _repo.DeleteAsync("invalid-id");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenOrderDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _repo.DeleteAsync(id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteOrder_WhenExists()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 123
        };

        await _repo.CreateAsync(order);

        var deleted = await _repo.DeleteAsync(order.Id);
        deleted.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(order.Id);
        fetched.Should().BeNull();
    }
}

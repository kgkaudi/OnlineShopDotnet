using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using OnlineShop.Api.Services;
using Xunit;

public class OrderServiceTests : RepositoryTestBase
{
    private readonly OrderService _service;
    private readonly OrderRepository _repo;
    private readonly IMongoCollection<Order> _orders;

    public OrderServiceTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new OrderRepository(config);
        _service = new OrderService(_repo);

        _orders = Fixture.Database.GetCollection<Order>("Orders");
        Fixture.Database.DropCollection("Orders");
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllOrders_ForAdmin()
    {
        var userA = ObjectId.GenerateNewId().ToString();
        var userB = ObjectId.GenerateNewId().ToString();

        await _repo.CreateAsync(new Order { Id = ObjectId.GenerateNewId().ToString(), UserId = userA });
        await _repo.CreateAsync(new Order { Id = ObjectId.GenerateNewId().ToString(), UserId = userB });

        var result = await _service.GetAllAsync(true, "ignored");

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnUserOrders_ForUser()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var otherId = ObjectId.GenerateNewId().ToString();

        await _repo.CreateAsync(new Order { Id = ObjectId.GenerateNewId().ToString(), UserId = userId });
        await _repo.CreateAsync(new Order { Id = ObjectId.GenerateNewId().ToString(), UserId = otherId });

        var result = await _service.GetAllAsync(false, userId);

        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(userId);
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdInvalid()
    {
        var result = await _service.GetByIdAsync("invalid-id", true, "ignored");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_ForAdmin()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId
        };

        await _repo.CreateAsync(order);

        var result = await _service.GetByIdAsync(order.Id, true, "ignored");
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserNotOwner()
    {
        var ownerId = ObjectId.GenerateNewId().ToString();
        var notOwnerId = ObjectId.GenerateNewId().ToString();

        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ownerId
        };

        await _repo.CreateAsync(order);

        var result = await _service.GetByIdAsync(order.Id, false, notOwnerId);
        result.Should().BeNull();
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldGenerateId_WhenMissing()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var order = new Order { UserId = userId };

        var created = await _service.CreateAsync(order);

        created.Id.Should().NotBeNull();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdInvalid()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var order = new Order { Id = "invalid-id", UserId = userId };

        var result = await _service.UpdateAsync(order, true, "ignored");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenOrderNotFound()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var order = new Order { Id = ObjectId.GenerateNewId().ToString(), UserId = userId };

        var result = await _service.UpdateAsync(order, true, "ignored");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdate_ForAdmin()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            Status = "Pending"
        };

        await _repo.CreateAsync(order);

        order.Status = "Shipped";

        var updated = await _service.UpdateAsync(order, true, "ignored");
        updated.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(order.Id);
        fetched!.Status.Should().Be("Shipped");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenUserNotOwner()
    {
        var ownerId = ObjectId.GenerateNewId().ToString();
        var notOwnerId = ObjectId.GenerateNewId().ToString();

        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ownerId
        };

        await _repo.CreateAsync(order);

        var result = await _service.UpdateAsync(order, false, notOwnerId);
        result.Should().BeFalse();
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdInvalid()
    {
        var result = await _service.DeleteAsync("invalid-id", true, "ignored");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenOrderNotFound()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.DeleteAsync(id, true, "ignored");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDelete_ForAdmin()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId
        };

        await _repo.CreateAsync(order);

        var deleted = await _service.DeleteAsync(order.Id, true, "ignored");
        deleted.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(order.Id);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenUserNotOwner()
    {
        var ownerId = ObjectId.GenerateNewId().ToString();
        var notOwnerId = ObjectId.GenerateNewId().ToString();

        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ownerId
        };

        await _repo.CreateAsync(order);

        var result = await _service.DeleteAsync(order.Id, false, notOwnerId);
        result.Should().BeFalse();
    }
}

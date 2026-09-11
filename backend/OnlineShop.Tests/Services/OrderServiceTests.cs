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
    // GET ALL — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoOrders()
    {
        var result = await _service.GetAllAsync(true, "ignored");
        result.Should().BeEmpty();
    }

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

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenUserIdInvalid()
    {
        var result = await _service.GetAllAsync(false, "invalid-id");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenUserIdNull()
    {
        var result = await _service.GetAllAsync(false, null!);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenUserIdWhitespace()
    {
        var result = await _service.GetAllAsync(false, "   ");
        result.Should().BeEmpty();
    }

    // ---------------------------------------------------------
    // GET BY ID — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdNull()
    {
        var result = await _service.GetByIdAsync(null!, true, "ignored");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdWhitespace()
    {
        var result = await _service.GetByIdAsync("   ", true, "ignored");
        result.Should().BeNull();
    }

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
    // CREATE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenOrderIsNull()
    {
        var result = await _service.CreateAsync(null!);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenUserIdNull()
    {
        var result = await _service.CreateAsync(new Order { UserId = null! });
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenUserIdWhitespace()
    {
        var result = await _service.CreateAsync(new Order { UserId = "   " });
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenUserIdInvalid()
    {
        var result = await _service.CreateAsync(new Order { UserId = "invalid-id" });
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateId_WhenMissing()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var order = new Order { UserId = userId };

        var created = await _service.CreateAsync(order);

        created.Should().NotBeNull();
        created!.Id.Should().NotBeNull();
    }

    // ---------------------------------------------------------
    // UPDATE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenOrderIsNull()
    {
        var result = await _service.UpdateAsync(null!, true, "ignored");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdNull()
    {
        var order = new Order { Id = null!, UserId = ObjectId.GenerateNewId().ToString() };
        var result = await _service.UpdateAsync(order, true, "ignored");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdWhitespace()
    {
        var order = new Order { Id = "   ", UserId = ObjectId.GenerateNewId().ToString() };
        var result = await _service.UpdateAsync(order, true, "ignored");
        result.Should().BeFalse();
    }

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
    // DELETE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdNull()
    {
        var result = await _service.DeleteAsync(null!, true, "ignored");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdWhitespace()
    {
        var result = await _service.DeleteAsync("   ", true, "ignored");
        result.Should().BeFalse();
    }

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

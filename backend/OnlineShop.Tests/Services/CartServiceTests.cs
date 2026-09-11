using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using OnlineShop.Api.Services;
using Xunit;

public class CartServiceTests : RepositoryTestBase
{
    private readonly CartService _service;
    private readonly CartRepository _repo;
    private readonly IMongoCollection<Cart> _carts;

    public CartServiceTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new CartRepository(config);
        _service = new CartService(_repo);

        _carts = Fixture.Database.GetCollection<Cart>("Carts");
        Fixture.Database.DropCollection("Carts");
    }

    // ---------------------------------------------------------
    // GET OR CREATE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task GetOrCreateAsync_ShouldThrow_WhenUserIdIsNull()
    {
        Func<Task> act = async () => await _service.GetOrCreateAsync(null!);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldThrow_WhenUserIdIsWhitespace()
    {
        Func<Task> act = async () => await _service.GetOrCreateAsync("   ");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldThrow_WhenUserIdIsInvalidObjectId()
    {
        Func<Task> act = async () => await _service.GetOrCreateAsync("invalid-id");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ---------------------------------------------------------
    // ADD ITEM — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task AddItemAsync_ShouldThrow_WhenUserIdIsNull()
    {
        Func<Task> act = async () => await _service.AddItemAsync(null!, ObjectId.GenerateNewId().ToString(), 1);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrow_WhenUserIdIsWhitespace()
    {
        Func<Task> act = async () => await _service.AddItemAsync("   ", ObjectId.GenerateNewId().ToString(), 1);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrow_WhenUserIdIsInvalidObjectId()
    {
        Func<Task> act = async () => await _service.AddItemAsync("invalid-id", ObjectId.GenerateNewId().ToString(), 1);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrow_WhenProductIdIsNull()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        Func<Task> act = async () => await _service.AddItemAsync(userId, null!, 1);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrow_WhenProductIdIsWhitespace()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        Func<Task> act = async () => await _service.AddItemAsync(userId, "   ", 1);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrow_WhenProductIdIsInvalidObjectId()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        Func<Task> act = async () => await _service.AddItemAsync(userId, "invalid-id", 1);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrow_WhenQuantityIsZero()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        Func<Task> act = async () => await _service.AddItemAsync(userId, productId, 0);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrow_WhenQuantityIsNegative()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        Func<Task> act = async () => await _service.AddItemAsync(userId, productId, -5);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ---------------------------------------------------------
    // UPDATE QUANTITY — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateQuantityAsync_ShouldThrow_WhenUserIdInvalid()
    {
        Func<Task> act = async () => await _service.UpdateQuantityAsync("invalid-id", ObjectId.GenerateNewId().ToString(), 5);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateQuantityAsync_ShouldThrow_WhenProductIdInvalid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        Func<Task> act = async () => await _service.UpdateQuantityAsync(userId, "invalid-id", 5);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateQuantityAsync_ShouldThrow_WhenQuantityIsZero()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        Func<Task> act = async () => await _service.UpdateQuantityAsync(userId, productId, 0);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateQuantityAsync_ShouldThrow_WhenQuantityIsNegative()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        Func<Task> act = async () => await _service.UpdateQuantityAsync(userId, productId, -10);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ---------------------------------------------------------
    // REMOVE ITEM — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task RemoveItemAsync_ShouldThrow_WhenUserIdInvalid()
    {
        Func<Task> act = async () => await _service.RemoveItemAsync("invalid-id", ObjectId.GenerateNewId().ToString());
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldThrow_WhenProductIdInvalid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        Func<Task> act = async () => await _service.RemoveItemAsync(userId, "invalid-id");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldDoNothing_WhenItemDoesNotExist()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        var cart = await _service.RemoveItemAsync(userId, productId);
        cart.Items.Should().BeEmpty();
    }

    // ---------------------------------------------------------
    // CLEAR — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task ClearAsync_ShouldThrow_WhenUserIdInvalid()
    {
        Func<Task> act = async () => await _service.ClearAsync("invalid-id");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ClearAsync_ShouldReturnFalse_WhenCartDoesNotExist()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var result = await _service.ClearAsync(userId);
        result.Should().BeFalse();
    }
}

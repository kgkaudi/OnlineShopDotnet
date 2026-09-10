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
    // GET OR CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task GetOrCreateAsync_ShouldCreateCart_WhenNoneExists()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var cart = await _service.GetOrCreateAsync(userId);

        cart.Should().NotBeNull();
        cart.UserId.Should().Be(userId);
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldReturnExistingCart()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var existing = new Cart
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            Items = new List<CartItem>()
        };

        await _repo.CreateAsync(existing);

        var cart = await _service.GetOrCreateAsync(userId);

        cart.Id.Should().Be(existing.Id);
    }

    // ---------------------------------------------------------
    // ADD ITEM
    // ---------------------------------------------------------

    [Fact]
    public async Task AddItemAsync_ShouldAddNewItem()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        var cart = await _service.AddItemAsync(userId, productId, 2);

        cart.Items.Should().HaveCount(1);
        cart.Items[0].ProductId.Should().Be(productId);
        cart.Items[0].Quantity.Should().Be(2);
    }

    [Fact]
    public async Task AddItemAsync_ShouldIncreaseQuantity_WhenItemExists()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        await _service.AddItemAsync(userId, productId, 2);
        var cart = await _service.AddItemAsync(userId, productId, 3);

        cart.Items.Should().HaveCount(1);
        cart.Items[0].Quantity.Should().Be(5);
    }

    // ---------------------------------------------------------
    // UPDATE QUANTITY
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateQuantityAsync_ShouldUpdateQuantity_WhenItemExists()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        await _service.AddItemAsync(userId, productId, 2);
        var cart = await _service.UpdateQuantityAsync(userId, productId, 10);

        cart.Items[0].Quantity.Should().Be(10);
    }

    [Fact]
    public async Task UpdateQuantityAsync_ShouldDoNothing_WhenItemDoesNotExist()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        var cart = await _service.UpdateQuantityAsync(userId, productId, 10);

        cart.Items.Should().BeEmpty();
    }

    // ---------------------------------------------------------
    // REMOVE ITEM
    // ---------------------------------------------------------

    [Fact]
    public async Task RemoveItemAsync_ShouldRemoveItem()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        await _service.AddItemAsync(userId, productId, 2);
        var cart = await _service.RemoveItemAsync(userId, productId);

        cart.Items.Should().BeEmpty();
    }

    // ---------------------------------------------------------
    // CLEAR
    // ---------------------------------------------------------

    [Fact]
    public async Task ClearAsync_ShouldClearCart()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        await _service.AddItemAsync(userId, productId, 2);

        var cleared = await _service.ClearAsync(userId);
        cleared.Should().BeTrue();

        var cart = await _repo.GetByUserIdAsync(userId);
        cart!.Items.Should().BeEmpty();
    }

    // ---------------------------------------------------------
    // INVALID IDS
    // ---------------------------------------------------------

    [Fact]
    public async Task GetOrCreateAsync_ShouldThrow_WhenUserIdInvalid()
    {
        Func<Task> act = async () => await _service.GetOrCreateAsync("invalid-id");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrow_WhenProductIdInvalid()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        Func<Task> act = async () => await _service.AddItemAsync(userId, "invalid-id", 2);
        await act.Should().ThrowAsync<ArgumentException>();
    }
}

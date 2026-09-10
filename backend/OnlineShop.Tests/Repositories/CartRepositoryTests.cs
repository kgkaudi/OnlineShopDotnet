using FluentAssertions;
using MongoDB.Bson;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using Xunit;

public class CartRepositoryTests : RepositoryTestBase
{
    private readonly CartRepository _repo;

    public CartRepositoryTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new CartRepository(config);

        // Ensure Carts collection is clean
        Fixture.Database.DropCollection("Carts");
    }

    // ---------------------------------------------------------
    // GET BY USER ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnNull_WhenCartDoesNotExist()
    {
        var result = await _repo.GetByUserIdAsync("missing-id");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnCart_WhenExists()
    {
        var cart = new Cart
        {
            UserId = ObjectId.GenerateNewId().ToString(),
            Items = new List<CartItem>()
        };

        await _repo.CreateAsync(cart);

        var result = await _repo.GetByUserIdAsync(cart.UserId);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(cart.UserId);
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldInsertCart()
    {
        var cart = new Cart
        {
            UserId = ObjectId.GenerateNewId().ToString(),
            Items = new List<CartItem>()
        };

        await _repo.CreateAsync(cart);

        var fetched = await _repo.GetByUserIdAsync(cart.UserId);
        fetched.Should().NotBeNull();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenCartIdIsInvalid()
    {
        var cart = new Cart
        {
            Id = "invalid-id",
            UserId = ObjectId.GenerateNewId().ToString(),
            Items = new List<CartItem>()
        };

        var result = await _repo.UpdateAsync(cart);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenCartDoesNotExist()
    {
        var cart = new Cart
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Items = new List<CartItem>()
        };

        var result = await _repo.UpdateAsync(cart);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCart_WhenExists()
    {
        var cart = new Cart
        {
            UserId = ObjectId.GenerateNewId().ToString(),
            Items = new List<CartItem>()
        };

        await _repo.CreateAsync(cart);

        cart.Items.Add(new CartItem
        {
            ProductId = ObjectId.GenerateNewId().ToString(),
            Quantity = 2
        });

        var updated = await _repo.UpdateAsync(cart);
        updated.Should().BeTrue();

        var fetched = await _repo.GetByUserIdAsync(cart.UserId);
        fetched!.Items.Should().HaveCount(1);
    }

    // ---------------------------------------------------------
    // CLEAR
    // ---------------------------------------------------------

    [Fact]
    public async Task ClearAsync_ShouldReturnFalse_WhenUserIdIsInvalid()
    {
        var result = await _repo.ClearAsync("invalid-id");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ClearAsync_ShouldReturnFalse_WhenCartDoesNotExist()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var result = await _repo.ClearAsync(userId);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ClearAsync_ShouldClearItems_WhenCartExists()
    {
        var cart = new Cart
        {
            UserId = ObjectId.GenerateNewId().ToString(),
            Items = new List<CartItem>
            {
                new CartItem
                {
                    ProductId = ObjectId.GenerateNewId().ToString(),
                    Quantity = 3
                }
            }
        };

        await _repo.CreateAsync(cart);

        var cleared = await _repo.ClearAsync(cart.UserId);
        cleared.Should().BeTrue();

        var fetched = await _repo.GetByUserIdAsync(cart.UserId);
        fetched!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearAsync_ShouldReturnTrue_WhenCartAlreadyEmpty()
    {
        var cart = new Cart
        {
            UserId = ObjectId.GenerateNewId().ToString(),
            Items = new List<CartItem>()
        };

        await _repo.CreateAsync(cart);

        var cleared = await _repo.ClearAsync(cart.UserId);
        cleared.Should().BeTrue();
    }
}

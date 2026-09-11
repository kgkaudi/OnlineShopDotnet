using FluentAssertions;
using MongoDB.Bson;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using MongoDB.Driver;
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

        Fixture.Database.DropCollection("Carts");
    }

    // ---------------------------------------------------------
    // GET BY USER ID
    // ---------------------------------------------------------

    [Fact] // [GetByUserId_NullUserId](ca://s?q=Test_GetByUserId_NullUserId)
    public async Task GetByUserIdAsync_ShouldReturnNull_WhenUserIdIsNull()
    {
        var result = await _repo.GetByUserIdAsync(null!);
        result.Should().BeNull();
    }

    [Fact] // [GetByUserId_Whitespace](ca://s?q=Test_GetByUserId_Whitespace)
    public async Task GetByUserIdAsync_ShouldReturnNull_WhenUserIdIsWhitespace()
    {
        var result = await _repo.GetByUserIdAsync("   ");
        result.Should().BeNull();
    }

    [Fact] // [GetByUserId_InvalidObjectId](ca://s?q=Test_GetByUserId_InvalidObjectId)
    public async Task GetByUserIdAsync_ShouldReturnNull_WhenUserIdIsInvalidObjectId()
    {
        var result = await _repo.GetByUserIdAsync("not-an-objectid");
        result.Should().BeNull();
    }

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

    [Fact] // [Create_NullCart](ca://s?q=Test_Create_NullCart)
    public async Task CreateAsync_ShouldThrow_WhenCartIsNull()
    {
        Func<Task> act = async () => await _repo.CreateAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact] // [Create_MissingUserId](ca://s?q=Test_Create_MissingUserId)
    public async Task CreateAsync_ShouldThrow_WhenUserIdIsMissing()
    {
        var cart = new Cart
        {
            UserId = null!,
            Items = new List<CartItem>()
        };

        Func<Task> act = async () => await _repo.CreateAsync(cart);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

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

    [Fact] // [Create_DuplicateUserId](ca://s?q=Test_Create_DuplicateUserId)
    public async Task CreateAsync_ShouldAllowDuplicateUserId()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        await _repo.CreateAsync(new Cart { UserId = userId, Items = new List<CartItem>() });
        await _repo.CreateAsync(new Cart { UserId = userId, Items = new List<CartItem>() });

        var carts = await Fixture.Database.GetCollection<Cart>("Carts")
            .Find(c => c.UserId == userId)
            .ToListAsync();

        carts.Should().HaveCount(2);
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact] // [Update_NullCart](ca://s?q=Test_Update_NullCart)
    public async Task UpdateAsync_ShouldReturnFalse_WhenCartIsNull()
    {
        var result = await _repo.UpdateAsync(null!);
        result.Should().BeFalse();
    }

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

    [Fact] // [Update_UserIdMismatch](ca://s?q=Test_Update_UserIdMismatch)
    public async Task UpdateAsync_ShouldReturnFalse_WhenUserIdMismatch()
    {
        var cart = new Cart
        {
            UserId = ObjectId.GenerateNewId().ToString(),
            Items = new List<CartItem>()
        };

        await _repo.CreateAsync(cart);

        cart.UserId = ObjectId.GenerateNewId().ToString(); // mismatch

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

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNothingChanged()
    {
        var cart = new Cart
        {
            UserId = ObjectId.GenerateNewId().ToString(),
            Items = new List<CartItem>()
        };

        await _repo.CreateAsync(cart);

        var updated = await _repo.UpdateAsync(cart);

        updated.Should().BeFalse();
    }

    [Fact] // [Update_NoModification](ca://s?q=Test_Update_NoModification)
    public async Task UpdateAsync_ShouldReturnFalse_WhenReplaceMatchedButNotModified()
    {
        var cart = new Cart
        {
            UserId = ObjectId.GenerateNewId().ToString(),
            Items = new List<CartItem>()
        };

        await _repo.CreateAsync(cart);

        var result = await _repo.UpdateAsync(cart);
        result.Should().BeFalse();
    }

    // ---------------------------------------------------------
    // CLEAR
    // ---------------------------------------------------------

    [Fact] // [Clear_NullUserId](ca://s?q=Test_Clear_NullUserId)
    public async Task ClearAsync_ShouldReturnFalse_WhenUserIdIsNull()
    {
        var result = await _repo.ClearAsync(null!);
        result.Should().BeFalse();
    }

    [Fact] // [Clear_Whitespace](ca://s?q=Test_Clear_Whitespace)
    public async Task ClearAsync_ShouldReturnFalse_WhenUserIdWhitespace()
    {
        var result = await _repo.ClearAsync("   ");
        result.Should().BeFalse();
    }

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

using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using OnlineShop.Api.Services;
using Xunit;

public class WishlistServiceTests : RepositoryTestBase
{
    private readonly WishlistService _service;
    private readonly WishlistRepository _repo;
    private readonly IMongoCollection<WishlistItem> _wishlist;

    public WishlistServiceTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new WishlistRepository(config);
        _service = new WishlistService(_repo);

        _wishlist = Fixture.Database.GetCollection<WishlistItem>("Wishlist");
        Fixture.Database.DropCollection("Wishlist");
    }

    // ---------------------------------------------------------
    // GET USER WISHLIST — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task GetUserWishlistAsync_ShouldReturnEmpty_WhenUserIdNull()
    {
        var result = await _service.GetUserWishlistAsync(null!);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserWishlistAsync_ShouldReturnEmpty_WhenUserIdWhitespace()
    {
        var result = await _service.GetUserWishlistAsync("   ");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserWishlistAsync_ShouldReturnEmpty_WhenUserIdInvalid()
    {
        var result = await _service.GetUserWishlistAsync("invalid-id");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserWishlistAsync_ShouldReturnItems_WhenUserMatches()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var otherId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        await _repo.AddAsync(new WishlistItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            ProductId = productId,
            AddedAt = DateTime.UtcNow
        });

        await _repo.AddAsync(new WishlistItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = otherId,
            ProductId = ObjectId.GenerateNewId().ToString(),
            AddedAt = DateTime.UtcNow
        });

        var result = await _service.GetUserWishlistAsync(userId);

        result.Should().HaveCount(1);
        result[0].ProductId.Should().Be(productId);
    }

    // ---------------------------------------------------------
    // ADD — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task AddAsync_ShouldReturnFalse_WhenUserIdNull()
    {
        var result = await _service.AddAsync(null!, ObjectId.GenerateNewId().ToString());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldReturnFalse_WhenUserIdWhitespace()
    {
        var result = await _service.AddAsync("   ", ObjectId.GenerateNewId().ToString());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldReturnFalse_WhenUserIdInvalid()
    {
        var result = await _service.AddAsync("invalid-id", ObjectId.GenerateNewId().ToString());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldReturnFalse_WhenProductIdNull()
    {
        var result = await _service.AddAsync(ObjectId.GenerateNewId().ToString(), null!);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldReturnFalse_WhenProductIdWhitespace()
    {
        var result = await _service.AddAsync(ObjectId.GenerateNewId().ToString(), "   ");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldReturnFalse_WhenProductIdInvalid()
    {
        var result = await _service.AddAsync(ObjectId.GenerateNewId().ToString(), "invalid-id");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldAddItem_WhenValid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        var added = await _service.AddAsync(userId, productId);
        added.Should().BeTrue();

        var result = await _repo.GetByUserIdAsync(userId);
        result.Should().HaveCount(1);
        result[0].ProductId.Should().Be(productId);
    }

    [Fact]
    public async Task AddAsync_ShouldNotDuplicateItem_WhenAlreadyExists()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        await _service.AddAsync(userId, productId);
        var addedAgain = await _service.AddAsync(userId, productId);

        addedAgain.Should().BeTrue();

        var result = await _repo.GetByUserIdAsync(userId);
        result.Should().HaveCount(1);
    }

    // ---------------------------------------------------------
    // REMOVE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenUserIdNull()
    {
        var result = await _service.RemoveAsync(null!, ObjectId.GenerateNewId().ToString());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenUserIdWhitespace()
    {
        var result = await _service.RemoveAsync("   ", ObjectId.GenerateNewId().ToString());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenUserIdInvalid()
    {
        var result = await _service.RemoveAsync("invalid-id", ObjectId.GenerateNewId().ToString());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenProductIdNull()
    {
        var result = await _service.RemoveAsync(ObjectId.GenerateNewId().ToString(), null!);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenProductIdWhitespace()
    {
        var result = await _service.RemoveAsync(ObjectId.GenerateNewId().ToString(), "   ");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenProductIdInvalid()
    {
        var result = await _service.RemoveAsync(ObjectId.GenerateNewId().ToString(), "invalid-id");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenItemNotFound()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        var result = await _service.RemoveAsync(userId, productId);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveItem_WhenExists()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        await _repo.AddAsync(new WishlistItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            ProductId = productId,
            AddedAt = DateTime.UtcNow
        });

        var removed = await _service.RemoveAsync(userId, productId);
        removed.Should().BeTrue();

        var result = await _repo.GetByUserIdAsync(userId);
        result.Should().BeEmpty();
    }
}

using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using Xunit;

public class WishlistRepositoryTests : RepositoryTestBase
{
    private readonly WishlistRepository _repo;
    private readonly IMongoCollection<WishlistItem> _wishlist;

    public WishlistRepositoryTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new WishlistRepository(config);
        _wishlist = Fixture.Database.GetCollection<WishlistItem>("Wishlist");

        Fixture.Database.DropCollection("Wishlist");
    }

    // ---------------------------------------------------------
    // GET BY USER ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserIdIsNull()
    {
        var result = await _repo.GetByUserIdAsync(null!);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserIdIsWhitespace()
    {
        var result = await _repo.GetByUserIdAsync("   ");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserIdIsInvalid()
    {
        var result = await _repo.GetByUserIdAsync("invalid-id");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserHasNoItems()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var result = await _repo.GetByUserIdAsync(userId);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnItems_WhenUserHasItems()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        await _repo.AddAsync(new WishlistItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            ProductId = ObjectId.GenerateNewId().ToString()
        });

        await _repo.AddAsync(new WishlistItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            ProductId = ObjectId.GenerateNewId().ToString()
        });

        var result = await _repo.GetByUserIdAsync(userId);
        result.Should().HaveCount(2);
    }

    // ---------------------------------------------------------
    // ADD
    // ---------------------------------------------------------

    [Fact]
    public async Task AddAsync_ShouldReturnFalse_WhenItemIsNull()
    {
        var result = await _repo.AddAsync(null!);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldReturnFalse_WhenIdIsMissing()
    {
        var item = new WishlistItem
        {
            Id = null!,
            UserId = ObjectId.GenerateNewId().ToString(),
            ProductId = ObjectId.GenerateNewId().ToString()
        };

        var result = await _repo.AddAsync(item);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldReturnFalse_WhenIdIsInvalid()
    {
        var item = new WishlistItem
        {
            Id = "invalid-id",
            UserId = ObjectId.GenerateNewId().ToString(),
            ProductId = ObjectId.GenerateNewId().ToString()
        };

        var result = await _repo.AddAsync(item);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldReturnFalse_WhenUserIdIsMissing()
    {
        var item = new WishlistItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = null!,
            ProductId = ObjectId.GenerateNewId().ToString()
        };

        var result = await _repo.AddAsync(item);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldReturnFalse_WhenUserIdIsInvalid()
    {
        var item = new WishlistItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = "invalid-id",
            ProductId = ObjectId.GenerateNewId().ToString()
        };

        var result = await _repo.AddAsync(item);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldReturnFalse_WhenProductIdIsMissing()
    {
        var item = new WishlistItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            ProductId = null!
        };

        var result = await _repo.AddAsync(item);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldReturnFalse_WhenProductIdIsInvalid()
    {
        var item = new WishlistItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            ProductId = "invalid-id"
        };

        var result = await _repo.AddAsync(item);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldInsertItem()
    {
        var item = new WishlistItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            ProductId = ObjectId.GenerateNewId().ToString()
        };

        var added = await _repo.AddAsync(item);
        added.Should().BeTrue();

        var fetched = await _wishlist.Find(w => w.Id == item.Id).FirstOrDefaultAsync();
        fetched.Should().NotBeNull();
    }

    // ---------------------------------------------------------
    // REMOVE
    // ---------------------------------------------------------

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenUserIdIsNull()
    {
        var result = await _repo.RemoveAsync(null!, ObjectId.GenerateNewId().ToString());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenUserIdIsWhitespace()
    {
        var result = await _repo.RemoveAsync("   ", ObjectId.GenerateNewId().ToString());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenUserIdIsInvalid()
    {
        var result = await _repo.RemoveAsync("invalid-id", ObjectId.GenerateNewId().ToString());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenProductIdIsNull()
    {
        var result = await _repo.RemoveAsync(ObjectId.GenerateNewId().ToString(), null!);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenProductIdIsWhitespace()
    {
        var result = await _repo.RemoveAsync(ObjectId.GenerateNewId().ToString(), "   ");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenProductIdIsInvalid()
    {
        var result = await _repo.RemoveAsync(ObjectId.GenerateNewId().ToString(), "invalid-id");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenItemDoesNotExist()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        var result = await _repo.RemoveAsync(userId, productId);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteItem_WhenExists()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        var item = new WishlistItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            ProductId = productId
        };

        await _repo.AddAsync(item);

        var deleted = await _repo.RemoveAsync(userId, productId);
        deleted.Should().BeTrue();

        var fetched = await _wishlist.Find(w => w.Id == item.Id).FirstOrDefaultAsync();
        fetched.Should().BeNull();
    }
}

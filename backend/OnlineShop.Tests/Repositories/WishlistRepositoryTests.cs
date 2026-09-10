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
    public async Task RemoveAsync_ShouldReturnFalse_WhenUserIdIsInvalid()
    {
        var result = await _repo.RemoveAsync("invalid-id", ObjectId.GenerateNewId().ToString());
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

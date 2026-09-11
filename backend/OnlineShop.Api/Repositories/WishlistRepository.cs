using MongoDB.Driver;
using MongoDB.Bson;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly IMongoCollection<WishlistItem> _wishlist;

    public WishlistRepository(IConfiguration config)
    {
        var connectionString = config["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionString missing in configuration.");

        var dbName = config["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDB:DatabaseName missing in configuration.");

        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(dbName);

        _wishlist = db.GetCollection<WishlistItem>("Wishlist");
    }

    // ---------------------------------------------------------
    // READ
    // ---------------------------------------------------------

    public async Task<List<WishlistItem>> GetByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new List<WishlistItem>();

        if (!ObjectId.TryParse(userId, out _))
            return new List<WishlistItem>();

        return await _wishlist.Find(w => w.UserId == userId).ToListAsync();
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task<bool> AddAsync(WishlistItem item)
    {
        if (item == null)
            return false;

        if (string.IsNullOrWhiteSpace(item.Id))
            return false;

        if (!ObjectId.TryParse(item.Id, out _))
            return false;

        if (string.IsNullOrWhiteSpace(item.UserId))
            return false;

        if (!ObjectId.TryParse(item.UserId, out _))
            return false;

        if (string.IsNullOrWhiteSpace(item.ProductId))
            return false;

        if (!ObjectId.TryParse(item.ProductId, out _))
            return false;

        await _wishlist.InsertOneAsync(item);
        return true;
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    public async Task<bool> RemoveAsync(string userId, string productId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        if (!ObjectId.TryParse(userId, out _))
            return false;

        if (string.IsNullOrWhiteSpace(productId))
            return false;

        if (!ObjectId.TryParse(productId, out _))
            return false;

        var result = await _wishlist.DeleteOneAsync(
            w => w.UserId == userId && w.ProductId == productId
        );

        return result.DeletedCount == 1;
    }
}

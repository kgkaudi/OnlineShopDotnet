using MongoDB.Driver;
using MongoDB.Bson;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly IMongoCollection<WishlistItem> _wishlist;

    public WishlistRepository(IConfiguration config)
    {
        var connectionUri = config["MongoDB:ConnectionURI"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionURI missing in configuration.");

        var dbName = config["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDB:DatabaseName missing in configuration.");

        var client = new MongoClient(connectionUri);
        var db = client.GetDatabase(dbName);

        _wishlist = db.GetCollection<WishlistItem>("Wishlist");
    }

    // ---------------------------------------------------------
    // READ
    // ---------------------------------------------------------

    public async Task<List<WishlistItem>> GetByUserIdAsync(string userId)
    {
        if (!ObjectId.TryParse(userId, out _))
            return new List<WishlistItem>();

        return await _wishlist.Find(w => w.UserId == userId).ToListAsync();
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task<bool> AddAsync(WishlistItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Id))
            item.Id = ObjectId.GenerateNewId().ToString();

        await _wishlist.InsertOneAsync(item);
        return true;
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    public async Task<bool> RemoveAsync(string userId, string productId)
    {
        if (!ObjectId.TryParse(userId, out _))
            return false;

        if (!ObjectId.TryParse(productId, out _))
            return false;

        var result = await _wishlist.DeleteOneAsync(
            w => w.UserId == userId && w.ProductId == productId
        );

        return result.DeletedCount > 0;
    }
}

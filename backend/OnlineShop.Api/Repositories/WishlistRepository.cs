using MongoDB.Driver;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly IMongoCollection<WishlistItem> _wishlist;

    public WishlistRepository(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionURI"]);
        var db = client.GetDatabase(config["MongoDB:DatabaseName"]);
        _wishlist = db.GetCollection<WishlistItem>("Wishlist");
    }

    public async Task<List<WishlistItem>> GetByUserIdAsync(string userId) =>
        await _wishlist.Find(w => w.UserId == userId).ToListAsync();

    public async Task<bool> AddAsync(WishlistItem item)
    {
        await _wishlist.InsertOneAsync(item);
        return true;
    }

    public async Task<bool> RemoveAsync(string userId, string productId)
    {
        var result = await _wishlist.DeleteOneAsync(
            w => w.UserId == userId && w.ProductId == productId
        );

        return result.DeletedCount > 0;
    }
}

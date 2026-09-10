using MongoDB.Driver;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class CartRepository : ICartRepository
{
    private readonly IMongoCollection<Cart> _carts;

    public CartRepository(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionURI"]);
        var db = client.GetDatabase(config["MongoDB:DatabaseName"]);
        _carts = db.GetCollection<Cart>("Carts");
    }

    public async Task<Cart?> GetByUserIdAsync(string userId) =>
        await _carts.Find(c => c.UserId == userId).FirstOrDefaultAsync();

    public async Task CreateAsync(Cart cart) =>
        await _carts.InsertOneAsync(cart);

    public async Task<bool> UpdateAsync(Cart cart)
    {
        var result = await _carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> ClearAsync(string userId)
    {
        var update = Builders<Cart>.Update.Set(c => c.Items, new List<CartItem>());
        var result = await _carts.UpdateOneAsync(c => c.UserId == userId, update);
        return result.ModifiedCount > 0;
    }
}

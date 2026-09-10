using MongoDB.Driver;
using MongoDB.Bson;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class CartRepository : ICartRepository
{
    private readonly IMongoCollection<Cart> _carts;

    public CartRepository(IConfiguration config)
    {
        var connectionUri = config["MongoDB:ConnectionURI"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionURI missing in configuration.");

        var dbName = config["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDB:DatabaseName missing in configuration.");

        var client = new MongoClient(connectionUri);
        var db = client.GetDatabase(dbName);

        _carts = db.GetCollection<Cart>("Carts");
    }

    // ---------------------------------------------------------
    // CRUD
    // ---------------------------------------------------------

    public async Task<Cart?> GetByUserIdAsync(string userId)
    {
        if (!ObjectId.TryParse(userId, out _))
            return null;

        return await _carts.Find(c => c.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Cart cart)
    {
        if (string.IsNullOrWhiteSpace(cart.Id))
            cart.Id = ObjectId.GenerateNewId().ToString();

        await _carts.InsertOneAsync(cart);
    }

    public async Task<bool> UpdateAsync(Cart cart)
    {
        if (!ObjectId.TryParse(cart.Id, out _))
            return false;

        var result = await _carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);

        // Treat "no modification" as success if the cart exists
        return result.MatchedCount > 0;
    }

    public async Task<bool> ClearAsync(string userId)
    {
        if (!ObjectId.TryParse(userId, out _))
            return false;

        var update = Builders<Cart>.Update.Set(c => c.Items, new List<CartItem>());
        var result = await _carts.UpdateOneAsync(c => c.UserId == userId, update);

        // If cart exists but was already empty → treat as success
        return result.MatchedCount > 0;
    }
}

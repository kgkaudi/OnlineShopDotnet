using MongoDB.Driver;
using MongoDB.Bson;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class CartRepository : ICartRepository
{
    private readonly IMongoCollection<Cart> _carts;

    public CartRepository(IConfiguration config)
    {
        var connectionString = config["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionString missing in configuration.");

        var dbName = config["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDB:DatabaseName missing in configuration.");

        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(dbName);

        _carts = db.GetCollection<Cart>("Carts");
    }

    // ---------------------------------------------------------
    // GET BY USER ID
    // ---------------------------------------------------------

    public async Task<Cart?> GetByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        if (!ObjectId.TryParse(userId, out _))
            return null;

        return await _carts.Find(c => c.UserId == userId).FirstOrDefaultAsync();
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task CreateAsync(Cart cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        if (string.IsNullOrWhiteSpace(cart.UserId))
            throw new ArgumentNullException(nameof(cart.UserId));

        if (string.IsNullOrWhiteSpace(cart.Id))
            cart.Id = ObjectId.GenerateNewId().ToString();

        await _carts.InsertOneAsync(cart);
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    public async Task<bool> UpdateAsync(Cart cart)
    {
        if (cart == null)
            return false;

        if (string.IsNullOrWhiteSpace(cart.Id))
            return false;

        if (!ObjectId.TryParse(cart.Id, out _))
            return false;

        if (string.IsNullOrWhiteSpace(cart.UserId))
            return false;

        // Ensure cart exists
        var existing = await GetByUserIdAsync(cart.UserId);
        if (existing == null)
            return false;

        // Ensure userId matches original cart
        if (existing.Id != cart.Id)
            return false;

        var result = await _carts.ReplaceOneAsync(
            c => c.Id == cart.Id,
            cart
        );

        // No match → fail
        if (result.MatchedCount == 0)
            return false;

        // If nothing changed → fail
        if (result.ModifiedCount == 0)
            return false;

        // Normal success
        return true;
    }

    // ---------------------------------------------------------
    // CLEAR
    // ---------------------------------------------------------

    public async Task<bool> ClearAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        if (!ObjectId.TryParse(userId, out _))
            return false;

        var cart = await GetByUserIdAsync(userId);
        if (cart == null)
            return false;

        cart.Items.Clear();

        var result = await _carts.ReplaceOneAsync(
            c => c.Id == cart.Id,
            cart
        );

        return result.MatchedCount == 1;
    }
}

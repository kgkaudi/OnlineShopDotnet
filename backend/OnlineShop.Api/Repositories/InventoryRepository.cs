using MongoDB.Driver;
using MongoDB.Bson;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IMongoCollection<Product> _products;

    public InventoryRepository(IConfiguration config)
    {
        var connectionString = config["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionString missing in configuration.");

        var dbName = config["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDB:DatabaseName missing in configuration.");

        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(dbName);

        _products = db.GetCollection<Product>("Products");
    }

    // ---------------------------------------------------------
    // INCREASE STOCK
    // ---------------------------------------------------------

    public async Task<bool> IncreaseStockAsync(string productId, int amount)
    {
        if (!ObjectId.TryParse(productId, out _))
            return false;

        var update = Builders<Product>.Update.Inc(p => p.StockQuantity, amount);
        var result = await _products.UpdateOneAsync(p => p.Id == productId, update);

        return result.MatchedCount > 0;
    }

    // ---------------------------------------------------------
    // DECREASE STOCK
    // ---------------------------------------------------------

    public async Task<bool> DecreaseStockAsync(string productId, int amount)
    {
        if (!ObjectId.TryParse(productId, out _))
            return false;

        var product = await _products.Find(p => p.Id == productId).FirstOrDefaultAsync();
        if (product == null)
            return false;

        if (product.StockQuantity < amount)
            return false;

        var update = Builders<Product>.Update.Inc(p => p.StockQuantity, -amount);
        var result = await _products.UpdateOneAsync(p => p.Id == productId, update);

        return result.MatchedCount > 0;
    }
}

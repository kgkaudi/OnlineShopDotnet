using MongoDB.Driver;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IMongoCollection<Product> _products;

    public InventoryRepository(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionURI"]);
        var db = client.GetDatabase(config["MongoDB:DatabaseName"]);
        _products = db.GetCollection<Product>("Products");
    }

    public async Task<bool> IncreaseStockAsync(string productId, int amount)
    {
        var update = Builders<Product>.Update.Inc(p => p.StockQuantity, amount);
        var result = await _products.UpdateOneAsync(p => p.Id == productId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DecreaseStockAsync(string productId, int amount)
    {
        var product = await _products.Find(p => p.Id == productId).FirstOrDefaultAsync();
        if (product == null || product.StockQuantity < amount)
            return false;

        var update = Builders<Product>.Update.Inc(p => p.StockQuantity, -amount);
        var result = await _products.UpdateOneAsync(p => p.Id == productId, update);
        return result.ModifiedCount > 0;
    }
}

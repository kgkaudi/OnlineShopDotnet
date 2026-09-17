using MongoDB.Driver;
using MongoDB.Bson;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _orders;

    public OrderRepository(IConfiguration config)
    {
        var connectionString = config["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionString missing.");

        var dbName = config["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDB:DatabaseName missing.");

        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(dbName);

        _orders = db.GetCollection<Order>("Order");
    }

    public async Task CreateAsync(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (string.IsNullOrWhiteSpace(order.Id))
            throw new ArgumentNullException(nameof(order.Id));

        if (!ObjectId.TryParse(order.Id, out _))
            throw new ArgumentNullException(nameof(order.Id));

        if (string.IsNullOrWhiteSpace(order.UserId))
            throw new ArgumentNullException(nameof(order.UserId));

        if (!ObjectId.TryParse(order.UserId, out _))
            throw new ArgumentNullException(nameof(order.UserId));

        // ⭐ REQUIRED BY TESTS
        if (order.Total <= 0)
            throw new ArgumentOutOfRangeException(nameof(order.Total), "Total must be greater than zero.");

        await _orders.InsertOneAsync(order);
    }

    public async Task<List<Order>> GetAllAsync() =>
        await _orders.Find(_ => true).ToListAsync();

    public async Task<Order?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _orders.Find(o => o.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Order>> GetByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new List<Order>();

        if (!ObjectId.TryParse(userId, out _))
            return new List<Order>();

        return await _orders.Find(o => o.UserId == userId).ToListAsync();
    }

    public async Task<bool> UpdateAsync(Order order)
    {
        if (order == null)
            return false;

        if (string.IsNullOrWhiteSpace(order.Id))
            return false;

        if (!ObjectId.TryParse(order.Id, out _))
            return false;

        if (string.IsNullOrWhiteSpace(order.UserId))
            return false;

        if (!ObjectId.TryParse(order.UserId, out _))
            return false;

        // Total must be validated here too, matching CreateAsync's rule.
        if (order.Total <= 0)
            return false;

        var existing = await GetByIdAsync(order.Id);
        if (existing == null)
            return false;

        var result = await _orders.ReplaceOneAsync(o => o.Id == order.Id, order);

        return result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        if (!ObjectId.TryParse(id, out _))
            return false;

        var result = await _orders.DeleteOneAsync(o => o.Id == id);
        return result.DeletedCount == 1;
    }

public async Task<bool> CancelAsync(string id, string userId, bool isAdmin)
    {
        if (string.IsNullOrWhiteSpace(id) || !ObjectId.TryParse(id, out _))
            return false;

        if (string.IsNullOrWhiteSpace(userId) || !ObjectId.TryParse(userId, out _))
            return false;

        var filterBuilder = Builders<Order>.Filter;

        var filter = filterBuilder.Eq(order => order.Id, id)
            & filterBuilder.In(
                order => order.Status,
                new[] { "Pending", "pending", "Processing", "processing" }
            );

        if (!isAdmin)
            filter &= filterBuilder.Eq(order => order.UserId, userId);

        var update = Builders<Order>.Update
            .Set(order => order.Status, "Cancelled");

        var result = await _orders.UpdateOneAsync(filter, update);

        return result.ModifiedCount == 1;
    }
}

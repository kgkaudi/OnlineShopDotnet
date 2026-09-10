using MongoDB.Driver;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _orders;

    public OrderRepository(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionURI"]);
        var db = client.GetDatabase(config["MongoDB:DatabaseName"]);
        _orders = db.GetCollection<Order>("Orders");
    }

    public async Task<List<Order>> GetAllAsync() =>
        await _orders.Find(_ => true).ToListAsync();

    public async Task<Order?> GetByIdAsync(string id) =>
        await _orders.Find(o => o.Id == id).FirstOrDefaultAsync();

    public async Task<List<Order>> GetByUserIdAsync(string userId) =>
        await _orders.Find(o => o.UserId == userId).ToListAsync();

    public async Task CreateAsync(Order order) =>
        await _orders.InsertOneAsync(order);

    public async Task<bool> UpdateAsync(Order order)
    {
        var result = await _orders.ReplaceOneAsync(o => o.Id == order.Id, order);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _orders.DeleteOneAsync(o => o.Id == id);
        return result.DeletedCount > 0;
    }
}

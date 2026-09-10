using MongoDB.Driver;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionURI"]);
        var db = client.GetDatabase(config["MongoDB:DatabaseName"]);
        _users = db.GetCollection<User>("Users");
    }

    public async Task<List<User>> GetAllAsync() =>
        await _users.Find(_ => true).ToListAsync();

    public async Task<User?> GetByIdAsync(string id) =>
        await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task<User?> GetByEmailAsync(string email) =>
        await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

    public async Task<bool> AddRoleAsync(string userId, string role)
    {
        var update = Builders<User>.Update.AddToSet(u => u.Roles, role);
        var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _users.DeleteOneAsync(u => u.Id == id);
        return result.DeletedCount > 0;
    }
}

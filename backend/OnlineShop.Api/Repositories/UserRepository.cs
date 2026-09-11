using MongoDB.Driver;
using MongoDB.Bson;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(IConfiguration config)
    {
        var connectionString = config["MongoDB:ConnectionString"]
            ?? throw new ArgumentNullException("MongoDB:ConnectionString");

        var dbName = config["MongoDB:DatabaseName"]
            ?? throw new ArgumentNullException("MongoDB:DatabaseName");

        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(dbName);

        _users = db.GetCollection<User>("Users");
    }

    public async Task CreateAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentNullException(nameof(user.Email));

        // REQUIRED BY TESTS
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            throw new ArgumentNullException(nameof(user.PasswordHash));

        if (string.IsNullOrWhiteSpace(user.Id))
            user.Id = ObjectId.GenerateNewId().ToString();

        if (!ObjectId.TryParse(user.Id, out _))
            throw new ArgumentNullException(nameof(user.Id));

        await _users.InsertOneAsync(user);
    }

    public async Task<List<User>> GetAllAsync() =>
        await _users.Find(_ => true).ToListAsync();

    public async Task<User?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<bool> AddRoleAsync(string userId, string role)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        if (!ObjectId.TryParse(userId, out _))
            return false;

        if (string.IsNullOrWhiteSpace(role))
            return false;

        var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null)
            return false;

        if (user.Roles.Contains(role))
            return true;

        var update = Builders<User>.Update.AddToSet(u => u.Roles, role);
        var result = await _users.UpdateOneAsync(u => u.Id == userId, update);

        return result.ModifiedCount == 1;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        if (!ObjectId.TryParse(id, out _))
            return false;

        var result = await _users.DeleteOneAsync(u => u.Id == id);
        return result.DeletedCount == 1;
    }
}

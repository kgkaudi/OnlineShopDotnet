using MongoDB.Driver;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class InvalidTokenRepository : IInvalidTokenRepository
{
    private readonly IMongoCollection<InvalidToken> _collection;

    public InvalidTokenRepository(IConfiguration config)
    {
        var connectionString = config["MongoDB:ConnectionString"]
            ?? throw new ArgumentNullException("MongoDB:ConnectionString", "MongoDB connection string is missing.");

        var databaseName = config["MongoDB:DatabaseName"]
            ?? throw new ArgumentNullException("MongoDB:DatabaseName", "MongoDB database name is missing.");

        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(databaseName);
        _collection = db.GetCollection<InvalidToken>("InvalidTokens");
    }

    public async Task AddAsync(string token)
    {
        await _collection.InsertOneAsync(new InvalidToken { Token = token });
    }

    public async Task<bool> ExistsAsync(string token)
    {
        return await _collection.Find(t => t.Token == token).AnyAsync();
    }
}

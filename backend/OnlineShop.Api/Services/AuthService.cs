using MongoDB.Driver;
using MongoDB.Bson;
using OnlineShop.Api.Models;
using System.Security.Cryptography;
using System.Text;

namespace OnlineShop.Api.Services;

public class AuthService : IAuthService
{
    private readonly IMongoCollection<User> _users;
    private readonly IJwtService _jwt;

    public AuthService(IConfiguration config, IJwtService jwt)
    {
        var connectionUri = config["MongoDB:ConnectionURI"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionURI missing in configuration.");

        var dbName = config["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDB:DatabaseName missing in configuration.");

        var client = new MongoClient(connectionUri);
        var db = client.GetDatabase(dbName);

        _users = db.GetCollection<User>("Users");
        _jwt = jwt;
    }

    // ---------------------------------------------------------
    // REGISTER
    // ---------------------------------------------------------

    public async Task<User?> RegisterAsync(string email, string password, string fullName)
    {
        var existing = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        if (existing != null)
            return null;

        var user = new User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = email,
            FullName = fullName,
            PasswordHash = HashPassword(password),
            Roles = new List<string> { "User" }
        };

        await _users.InsertOneAsync(user);
        return user;
    }

    // ---------------------------------------------------------
    // LOGIN
    // ---------------------------------------------------------

    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null)
            return null;

        if (!VerifyPassword(password, user.PasswordHash))
            return null;

        return _jwt.GenerateToken(user);
    }

    // ---------------------------------------------------------
    // ADD ROLE
    // ---------------------------------------------------------

    public async Task<bool> AddRoleAsync(string userId, string role)
    {
        if (!ObjectId.TryParse(userId, out _))
            return false;

        var update = Builders<User>.Update.AddToSet(u => u.Roles, role);
        var result = await _users.UpdateOneAsync(u => u.Id == userId, update);

        return result.MatchedCount > 0;
    }

    // ---------------------------------------------------------
    // PASSWORD HASHING
    // ---------------------------------------------------------

    private string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(
            sha.ComputeHash(Encoding.UTF8.GetBytes(password))
        );
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        return storedHash == HashPassword(password);
    }
}

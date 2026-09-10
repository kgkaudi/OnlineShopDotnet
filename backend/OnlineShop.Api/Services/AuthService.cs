using MongoDB.Driver;
using OnlineShop.Api.Models;
using System.Security.Cryptography;
using System.Text;

namespace OnlineShop.Api.Services;

public class AuthService
{
    private readonly IMongoCollection<User> _users;
    private readonly IJwtService _jwt;

    public AuthService(IConfiguration config, IJwtService jwt)
    {
        var client = new MongoClient(config["MongoDB:ConnectionURI"]);
        var db = client.GetDatabase(config["MongoDB:DatabaseName"]);

        _users = db.GetCollection<User>("Users");
        _jwt = jwt;
    }

    // -----------------------------
    // Register new user
    // -----------------------------
    public async Task<User?> RegisterAsync(string email, string password, string fullName)
    {
        var existing = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        if (existing != null)
            return null;

        var user = new User
        {
            Email = email,
            FullName = fullName,
            PasswordHash = HashPassword(password),
            Roles = new List<string> { "User" }
        };

        await _users.InsertOneAsync(user);
        return user;
    }

    // -----------------------------
    // Login user
    // -----------------------------
    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null)
            return null;

        if (!VerifyPassword(password, user.PasswordHash))
            return null;

        return _jwt.GenerateToken(user);
    }

    // -----------------------------
    // Promote user to a role
    // -----------------------------
    public async Task<bool> AddRoleAsync(string userId, string role)
    {
        var update = Builders<User>.Update.AddToSet(u => u.Roles, role);
        var result = await _users.UpdateOneAsync(u => u.Id == userId, update);

        return result.ModifiedCount > 0;
    }

    // -----------------------------
    // Password hashing
    // -----------------------------
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

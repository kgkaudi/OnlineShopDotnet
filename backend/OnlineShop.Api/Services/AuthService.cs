using MongoDB.Bson;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using Microsoft.Extensions.Configuration;

namespace OnlineShop.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly IJwtService _jwt;
    private readonly IConfiguration _config;

    public AuthService(IConfiguration config, IJwtService jwt, IUserRepository repo)
    {
        _config = config;
        _jwt = jwt;
        _repo = repo;
    }

    public async Task<User?> RegisterAsync(string email, string password, string fullName)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(fullName))
            return null;

        var normalizedEmail = email.Trim().ToLowerInvariant();

        var existing = await _repo.GetByEmailAsync(normalizedEmail);
        if (existing != null)
            return null;

        var user = new User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = normalizedEmail,
            FullName = fullName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Roles = new List<string> { "User" }
        };

        await _repo.CreateAsync(user);
        return user;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        var trimmedEmail = email.Trim();
        var user = await _repo.GetByEmailAsync(trimmedEmail);
        if (user == null)
            return null;

        // Reject legacy SHA256 hashes
        if (!IsValidBcryptHash(user.PasswordHash))
            return null;

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return _jwt.GenerateToken(user);
    }

    private bool IsValidBcryptHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return false;

        return hash.StartsWith("$2a$") ||
               hash.StartsWith("$2b$") ||
               hash.StartsWith("$2y$");
    }

    public async Task<bool> AddRoleAsync(string userId, string role)
    {
        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(role) ||
            !ObjectId.TryParse(userId, out _))
            return false;

        return await _repo.AddRoleAsync(userId, role);
    }
}

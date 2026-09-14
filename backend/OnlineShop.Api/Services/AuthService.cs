using MongoDB.Bson;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using Microsoft.Extensions.Configuration;

namespace OnlineShop.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly IInvalidTokenRepository _invalidTokens;
    private readonly IJwtService _jwt;
    private readonly IConfiguration _config;

    public AuthService(IConfiguration config, IJwtService jwt, IUserRepository repo, IInvalidTokenRepository invalidTokens)
    {
        _config = config;
        _jwt = jwt;
        _repo = repo;
        _invalidTokens = invalidTokens;
    }

    // REGISTER
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

    // LOGIN
    public async Task<string?> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        var trimmedEmail = email.Trim();
        var user = await _repo.GetByEmailAsync(trimmedEmail);
        if (user == null)
            return null;

        if (!IsValidBcryptHash(user.PasswordHash))
            return null;

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

    // ADD ROLE
    public async Task<bool> AddRoleAsync(string userId, string role)
    {
        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(role) ||
            !ObjectId.TryParse(userId, out _))
            return false;

        return await _repo.AddRoleAsync(userId, role);
    }

    // LOGOUT — REAL TOKEN INVALIDATION
    public async Task<bool> LogoutAsync(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return false;

        await _invalidTokens.AddAsync(token); // store invalid token
        return true;
    }
}

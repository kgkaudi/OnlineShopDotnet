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
        var existing = await _repo.GetByEmailAsync(email);
        if (existing != null)
            return null;

        var user = new User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = email,
            FullName = fullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Roles = new List<string> { "User" }
        };

        await _repo.CreateAsync(user);
        return user;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _repo.GetByEmailAsync(email);
        if (user == null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return _jwt.GenerateToken(user);
    }

    public async Task<bool> AddRoleAsync(string userId, string role)
    {
        if (!ObjectId.TryParse(userId, out _))
            return false;

        return await _repo.AddRoleAsync(userId, role);
    }
}

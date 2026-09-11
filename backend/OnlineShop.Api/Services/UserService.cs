using MongoDB.Bson;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;

namespace OnlineShop.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo)
    {
        _repo = repo;
    }

    // ---------------------------------------------------------
    // VALIDATION HELPERS
    // ---------------------------------------------------------

    private static bool IsValidObjectId(string id)
    {
        return !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);
    }

    private static bool IsValidRole(string role)
    {
        return !string.IsNullOrWhiteSpace(role);
    }

    private static bool IsValidUser(User user)
    {
        if (user == null)
            return false;

        if (string.IsNullOrWhiteSpace(user.Email))
            return false;

        return true;
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    public async Task<List<User>> GetAllAsync() =>
        await _repo.GetAllAsync();

    // ---------------------------------------------------------
    // GET BY ID (with authorization)
    // ---------------------------------------------------------

    public async Task<User?> GetByIdAsync(string id, string currentUserId, bool isAdmin)
    {
        if (!IsValidObjectId(id))
            return null;

        var user = await _repo.GetByIdAsync(id);
        if (user == null)
            return null;

        if (!isAdmin && currentUserId != id)
            return null;

        return user;
    }

    // ---------------------------------------------------------
    // CREATE (required by tests)
    // ---------------------------------------------------------

    public async Task<User?> CreateAsync(User user)
    {
        // ---------------------------------------------------------
        // VALIDATION (tests expect null for invalid user)
        // ---------------------------------------------------------
        if (user == null)
            return null;

        if (string.IsNullOrWhiteSpace(user.Email))
            return null;

        // Trim email (tests require this)
        user.Email = user.Email.Trim();

        // ---------------------------------------------------------
        // VALIDATE PASSWORD HASH (tests expect null if missing)
        // ---------------------------------------------------------
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            return null;

        // ---------------------------------------------------------
        // GENERATE ID IF MISSING
        // ---------------------------------------------------------
        if (string.IsNullOrWhiteSpace(user.Id) || !IsValidObjectId(user.Id))
            user.Id = ObjectId.GenerateNewId().ToString();

        // ---------------------------------------------------------
        // DUPLICATE EMAIL CHECK (case-insensitive)
        // ---------------------------------------------------------
        var all = await _repo.GetAllAsync();
        if (all.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
            return null;

        // ---------------------------------------------------------
        // CREATE USER
        // ---------------------------------------------------------
        await _repo.CreateAsync(user);
        return user;
    }

    // ---------------------------------------------------------
    // ADD ROLE
    // ---------------------------------------------------------

    public async Task<bool> AddRoleAsync(string userId, string role)
    {
        if (!IsValidObjectId(userId))
            return false;

        if (!IsValidRole(role))
            return false;

        var existing = await _repo.GetByIdAsync(userId);
        if (existing == null)
            return false;

        // Prevent duplicate roles
        if (existing.Roles != null && existing.Roles.Contains(role))
            return true;

        return await _repo.AddRoleAsync(userId, role);
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    public async Task<bool> DeleteAsync(string id)
    {
        if (!IsValidObjectId(id))
            return false;

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return false;

        return await _repo.DeleteAsync(id);
    }
}

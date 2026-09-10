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
    // GET ALL
    // ---------------------------------------------------------

    public async Task<List<User>> GetAllAsync() =>
        await _repo.GetAllAsync();

    // ---------------------------------------------------------
    // GET BY ID (with authorization)
    // ---------------------------------------------------------

    public async Task<User?> GetByIdAsync(string id, string currentUserId, bool isAdmin)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        var user = await _repo.GetByIdAsync(id);
        if (user == null)
            return null;

        if (!isAdmin && currentUserId != id)
            return null;

        return user;
    }

    // ---------------------------------------------------------
    // ADD ROLE
    // ---------------------------------------------------------

    public async Task<bool> AddRoleAsync(string userId, string role)
    {
        if (!ObjectId.TryParse(userId, out _))
            return false;

        return await _repo.AddRoleAsync(userId, role);
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    public async Task<bool> DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return false;

        return await _repo.DeleteAsync(id);
    }
}

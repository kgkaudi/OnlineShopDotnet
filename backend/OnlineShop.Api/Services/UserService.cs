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

    public async Task<List<User>> GetAllAsync() =>
        await _repo.GetAllAsync();

    public async Task<User?> GetByIdAsync(string id, string currentUserId, bool isAdmin)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return null;

        if (!isAdmin && currentUserId != id)
            return null;

        return user;
    }

    public async Task<bool> AddRoleAsync(string userId, string role) =>
        await _repo.AddRoleAsync(userId, role);

    public async Task<bool> DeleteAsync(string id) =>
        await _repo.DeleteAsync(id);
}

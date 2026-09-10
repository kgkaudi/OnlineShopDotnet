using OnlineShop.Api.Models;

namespace OnlineShop.Api.Services;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(string id, string currentUserId, bool isAdmin);
    Task<bool> AddRoleAsync(string userId, string role);
    Task<bool> DeleteAsync(string id);
}

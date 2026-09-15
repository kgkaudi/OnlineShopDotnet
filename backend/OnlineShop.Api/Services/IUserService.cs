using OnlineShop.Api.Models;

namespace OnlineShop.Api.Services;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(string id, string currentUserId, bool isAdmin);

    Task<User?> CreateAsync(User user);
    Task<User?> UpdateProfileAsync(string id, string fullName, string email);

    Task<bool> AddRoleAsync(string userId, string role);
    Task<bool> DeleteAsync(string id);
}

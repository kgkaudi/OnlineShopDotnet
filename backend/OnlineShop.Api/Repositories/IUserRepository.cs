using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<List<User>> GetAllAsync();
    Task CreateAsync(User user);
    Task<bool> AddRoleAsync(string userId, string role);
    Task<bool> DeleteAsync(string id);
}

using OnlineShop.Api.Models;

namespace OnlineShop.Api.Services;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(string id);
    Task<Category?> CreateAsync(string name);
    Task<bool> UpdateAsync(string id, string name);
    Task<bool> DeleteAsync(string id);
}

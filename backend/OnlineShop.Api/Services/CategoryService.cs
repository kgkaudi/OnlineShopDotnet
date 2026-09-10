using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;

namespace OnlineShop.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<Category>> GetAllAsync() =>
        await _repo.GetAllAsync();

    public async Task<Category?> GetByIdAsync(string id) =>
        await _repo.GetByIdAsync(id);

    public async Task<Category> CreateAsync(Category category)
    {
        await _repo.CreateAsync(category);
        return category;
    }

    public async Task<bool> UpdateAsync(Category category) =>
        await _repo.UpdateAsync(category);

    public async Task<bool> DeleteAsync(string id) =>
        await _repo.DeleteAsync(id);
}

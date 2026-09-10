using MongoDB.Bson;
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

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    public async Task<List<Category>> GetAllAsync() =>
        await _repo.GetAllAsync();

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    public async Task<Category?> GetByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _repo.GetByIdAsync(id);
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task<Category?> CreateAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var category = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = name
        };

        await _repo.CreateAsync(category);
        return category;
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    public async Task<bool> UpdateAsync(string id, string name)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return false;

        existing.Name = name;

        return await _repo.UpdateAsync(existing);
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    public async Task<bool> DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        return await _repo.DeleteAsync(id);
    }
}

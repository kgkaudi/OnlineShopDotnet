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
    // VALIDATION HELPERS
    // ---------------------------------------------------------

    private static string? NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return name.Trim();
    }

    private static bool IsValidObjectId(string id)
    {
        return !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);
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
        if (!IsValidObjectId(id))
            return null;

        return await _repo.GetByIdAsync(id);
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task<Category?> CreateAsync(string name)
    {
        var normalized = NormalizeName(name);
        if (normalized == null)
            return null;

        // Prevent duplicate names
        var existing = await _repo.GetAllAsync();
        if (existing.Any(c => c.Name.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
            return null;

        var category = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = normalized
        };

        await _repo.CreateAsync(category);
        return category;
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    public async Task<bool> UpdateAsync(string id, string name)
    {
        if (!IsValidObjectId(id))
            return false;

        var normalized = NormalizeName(name);
        if (normalized == null)
            return false;

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return false;

        // Prevent unchanged update
        if (existing.Name.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            return false;

        // Prevent duplicate name
        var all = await _repo.GetAllAsync();
        if (all.Any(c => c.Id != id && c.Name.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
            return false;

        existing.Name = normalized;

        return await _repo.UpdateAsync(existing);
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    public async Task<bool> DeleteAsync(string id)
    {
        if (!IsValidObjectId(id))
            return false;

        return await _repo.DeleteAsync(id);
    }
}

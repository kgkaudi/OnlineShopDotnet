using MongoDB.Bson;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;

namespace OnlineShop.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
    }

    // ---------------------------------------------------------
    // VALIDATION HELPERS
    // ---------------------------------------------------------

    private static string? NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return name.Trim();
    }

    private static bool IsValidObjectId(string? id)
    {
        return !string.IsNullOrWhiteSpace(id)
               && ObjectId.TryParse(id, out _);
    }

    private static bool NamesEqual(string? first, string? second)
    {
        return string.Equals(
            first?.Trim(),
            second?.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    public async Task<List<Category>> GetAllAsync()
    {
        var categories = await _repo.GetAllAsync();

        return categories ?? new List<Category>();
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    public async Task<Category?> GetByIdAsync(string? id)
    {
        if (!IsValidObjectId(id))
            return null;

        return await _repo.GetByIdAsync(id!.Trim());
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task<Category?> CreateAsync(string? name)
    {
        var normalizedName = NormalizeName(name);

        if (normalizedName == null)
            return null;

        // Prevent duplicate category names.
        var existingCategories = await _repo.GetAllAsync();

        if (existingCategories.Any(category =>
                NamesEqual(category.Name, normalizedName)))
        {
            return null;
        }

        var category = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = normalizedName
        };

        await _repo.CreateAsync(category);

        return category;
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    public async Task<bool> UpdateAsync(string? id, string? name)
    {
        if (!IsValidObjectId(id))
            return false;

        var normalizedName = NormalizeName(name);

        if (normalizedName == null)
            return false;

        var normalizedId = id!.Trim();

        var existingCategory = await _repo.GetByIdAsync(normalizedId);

        if (existingCategory == null)
            return false;

        // No change required.
        if (NamesEqual(existingCategory.Name, normalizedName))
            return false;

        // Prevent another category from using the same name.
        var allCategories = await _repo.GetAllAsync();

        var duplicateExists = allCategories.Any(category =>
            !string.Equals(category.Id, normalizedId, StringComparison.OrdinalIgnoreCase) &&
            NamesEqual(category.Name, normalizedName));

        if (duplicateExists)
            return false;

        existingCategory.Name = normalizedName;

        return await _repo.UpdateAsync(existingCategory);
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    public async Task<bool> DeleteAsync(string? id)
    {
        if (!IsValidObjectId(id))
            return false;

        return await _repo.DeleteAsync(id!.Trim());
    }
}

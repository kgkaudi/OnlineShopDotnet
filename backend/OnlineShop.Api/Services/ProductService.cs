using MongoDB.Bson;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;

namespace OnlineShop.Api.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    // ---------------------------------------------------------
    // VALIDATION HELPERS
    // ---------------------------------------------------------

    private static bool IsValidObjectId(string id)
    {
        return !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);
    }

    private static bool IsValidProduct(Product product)
    {
        if (product == null)
            return false;

        if (string.IsNullOrWhiteSpace(product.Name))
            return false;

        if (product.Price <= 0)
            return false;

        if (product.StockQuantity < 0)
            return false;

        if (!string.IsNullOrWhiteSpace(product.CategoryId) &&
            !IsValidObjectId(product.CategoryId))
            return false;

        return true;
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    public async Task<List<Product>> GetAllAsync() =>
        await _repo.GetAllAsync();

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    public async Task<Product?> GetByIdAsync(string id)
    {
        if (!IsValidObjectId(id))
            return null;

        return await _repo.GetByIdAsync(id);
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task<Product?> CreateAsync(Product product)
    {
        if (!IsValidProduct(product))
            return null;

        product.Name = product.Name.Trim();

        if (!IsValidObjectId(product.Id))
            product.Id = ObjectId.GenerateNewId().ToString();

        await _repo.CreateAsync(product);
        return product;
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    public async Task<bool> UpdateAsync(Product product)
    {
        if (product == null)
            return false;

        if (!IsValidObjectId(product.Id))
            return false;

        var existing = await _repo.GetByIdAsync(product.Id);
        if (existing == null)
            return false;

        if (!IsValidProduct(product))
            return false;

        product.Name = product.Name.Trim();

        return await _repo.UpdateAsync(product);
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    public async Task<bool> DeleteAsync(string id)
    {
        if (!IsValidObjectId(id))
            return false;

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return false;

        return await _repo.DeleteAsync(id);
    }

    // ---------------------------------------------------------
    // SEARCH + FILTERING
    // ---------------------------------------------------------

    public async Task<List<Product>> SearchAsync(
        string? keyword,
        string? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        bool? inStock,
        string? sortBy,
        bool descending
    )
    {
        // Keyword normalization
        if (string.IsNullOrWhiteSpace(keyword))
            keyword = null;
        else
            keyword = keyword.Trim();

        // Category validation
        if (!string.IsNullOrWhiteSpace(categoryId) && !IsValidObjectId(categoryId))
            return new List<Product>();

        return await _repo.SearchAsync(
            keyword,
            categoryId,
            minPrice,
            maxPrice,
            inStock,
            sortBy,
            descending
        );
    }
}

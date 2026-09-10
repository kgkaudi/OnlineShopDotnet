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
    // CRUD
    // ---------------------------------------------------------

    public async Task<List<Product>> GetAllAsync() =>
        await _repo.GetAllAsync();

    public async Task<Product?> GetByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _repo.GetByIdAsync(id);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Id))
            product.Id = ObjectId.GenerateNewId().ToString();

        await _repo.CreateAsync(product);
        return product;
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        if (!ObjectId.TryParse(product.Id, out _))
            return false;

        var existing = await _repo.GetByIdAsync(product.Id!);
        if (existing == null)
            return false;

        return await _repo.UpdateAsync(product);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
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

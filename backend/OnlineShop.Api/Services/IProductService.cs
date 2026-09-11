using OnlineShop.Api.Models;

namespace OnlineShop.Api.Services;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(string id);

    // Updated to match strict‑validation ProductService
    Task<Product?> CreateAsync(Product product);

    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(string id);

    Task<List<Product>> SearchAsync(
        string? keyword,
        string? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        bool? inStock,
        string? sortBy,
        bool descending
    );
}

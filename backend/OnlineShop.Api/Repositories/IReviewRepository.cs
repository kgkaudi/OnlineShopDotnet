using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public interface IReviewRepository
{
    Task<List<Review>> GetByProductIdAsync(string productId);
    Task<Review?> GetByIdAsync(string id);
    Task CreateAsync(Review review);
    Task<bool> DeleteAsync(string id);
}

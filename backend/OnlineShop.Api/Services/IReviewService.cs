using OnlineShop.Api.Models;

namespace OnlineShop.Api.Services;

public interface IReviewService
{
    Task<List<Review>> GetByProductIdAsync(string productId);
    Task<Review> CreateAsync(Review review);
    Task<bool> DeleteAsync(string id);
}

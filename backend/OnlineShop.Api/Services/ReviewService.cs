using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;

namespace OnlineShop.Api.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _repo;

    public ReviewService(IReviewRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<Review>> GetByProductIdAsync(string productId) =>
        await _repo.GetByProductIdAsync(productId);

    public async Task<Review> CreateAsync(Review review)
    {
        await _repo.CreateAsync(review);
        return review;
    }

    public async Task<bool> DeleteAsync(string id) =>
        await _repo.DeleteAsync(id);
}

using MongoDB.Bson;
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

    // ---------------------------------------------------------
    // GET BY PRODUCT
    // ---------------------------------------------------------

    public async Task<List<Review>> GetByProductIdAsync(string productId)
    {
        if (!ObjectId.TryParse(productId, out _))
            return new List<Review>();

        return await _repo.GetByProductIdAsync(productId);
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task<Review> CreateAsync(Review review)
    {
        if (string.IsNullOrWhiteSpace(review.Id))
            review.Id = ObjectId.GenerateNewId().ToString();

        if (!ObjectId.TryParse(review.ProductId, out _))
            throw new ArgumentException("Invalid ProductId", nameof(review.ProductId));

        if (!ObjectId.TryParse(review.UserId, out _))
            throw new ArgumentException("Invalid UserId", nameof(review.UserId));

        await _repo.CreateAsync(review);
        return review;
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    public async Task<bool> DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return false;

        return await _repo.DeleteAsync(id);
    }
}

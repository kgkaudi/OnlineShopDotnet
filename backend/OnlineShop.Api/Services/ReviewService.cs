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
    // VALIDATION HELPERS
    // ---------------------------------------------------------

    private static bool IsValidObjectId(string id)
    {
        return !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);
    }

    // ---------------------------------------------------------
    // GET ALL (Admin)
    // ---------------------------------------------------------

    public async Task<List<Review>> GetAllAsync()
    {
        return await _repo.GetAllAsync();
    }

    // ---------------------------------------------------------
    // GET BY PRODUCT
    // ---------------------------------------------------------

    public async Task<List<Review>> GetByProductIdAsync(string productId)
    {
        if (!IsValidObjectId(productId))
            return new List<Review>();

        return await _repo.GetByProductIdAsync(productId);
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task<Review?> CreateAsync(Review? review)
    {
        if (review == null)
            return null;

        // ProductId must be valid
        if (!IsValidObjectId(review.ProductId))
            throw new ArgumentException("Invalid review data.");

        // UserId is already assigned by the controller → no need to validate here

        // Comment required
        if (string.IsNullOrWhiteSpace(review.Comment))
            throw new ArgumentException("Invalid review data.");

        // Rating must be 1–5
        if (review.Rating < 1 || review.Rating > 5)
            throw new ArgumentException("Invalid review data.");

        // Trim comment
        review.Comment = review.Comment.Trim();

        // Generate ID if missing
        if (!IsValidObjectId(review.Id))
            review.Id = ObjectId.GenerateNewId().ToString();

        await _repo.CreateAsync(review);
        return review;
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
}

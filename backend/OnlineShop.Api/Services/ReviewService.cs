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

    private static bool IsValidReview(Review review)
    {
        if (review == null)
            return false;

        if (!IsValidObjectId(review.ProductId))
            return false;

        if (!IsValidObjectId(review.UserId))
            return false;

        if (review.Rating <= 0)
            return false;

        if (string.IsNullOrWhiteSpace(review.Comment))
            return false;

        return true;
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
        // ---------------------------------------------------------
        // NULL REVIEW → return null (tests expect this)
        // ---------------------------------------------------------
        if (review == null)
            return null;

        // ---------------------------------------------------------
        // VALIDATION (tests expect ArgumentException for invalid data)
        // ---------------------------------------------------------
        if (!IsValidObjectId(review.ProductId))
            throw new ArgumentException("Invalid review data.");

        if (!IsValidObjectId(review.UserId))
            throw new ArgumentException("Invalid review data.");

        if (string.IsNullOrWhiteSpace(review.Comment))
            throw new ArgumentException("Invalid review data.");

        if (review.Rating < 1 || review.Rating > 5)
            throw new ArgumentException("Invalid review data.");

        // Trim comment
        review.Comment = review.Comment.Trim();

        // Generate ID if missing
        if (!IsValidObjectId(review.Id))
            review.Id = ObjectId.GenerateNewId().ToString();

        // ---------------------------------------------------------
        // SAVE
        // ---------------------------------------------------------
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

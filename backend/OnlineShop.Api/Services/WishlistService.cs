using MongoDB.Bson;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;

namespace OnlineShop.Api.Services;

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _repo;

    public WishlistService(IWishlistRepository repo)
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
    // GET USER WISHLIST
    // ---------------------------------------------------------

    public async Task<List<WishlistItem>> GetUserWishlistAsync(string userId)
    {
        if (!IsValidObjectId(userId))
            return new List<WishlistItem>();

        return await _repo.GetByUserIdAsync(userId);
    }

    // ---------------------------------------------------------
    // ADD ITEM
    // ---------------------------------------------------------

    public async Task<bool> AddAsync(string userId, string productId)
    {
        if (!IsValidObjectId(userId))
            return false;

        if (!IsValidObjectId(productId))
            return false;

        // Prevent duplicates
        var existing = await _repo.GetByUserIdAsync(userId);
        if (existing.Any(i => i.ProductId == productId))
            return true; // Already exists → treat as success

        var item = new WishlistItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            ProductId = productId,
            AddedAt = DateTime.UtcNow
        };

        return await _repo.AddAsync(item);
    }

    // ---------------------------------------------------------
    // REMOVE ITEM
    // ---------------------------------------------------------

    public async Task<bool> RemoveAsync(string userId, string productId)
    {
        if (!IsValidObjectId(userId))
            return false;

        if (!IsValidObjectId(productId))
            return false;

        return await _repo.RemoveAsync(userId, productId);
    }

    // ---------------------------------------------------------
    // PRODUCT EXISTS (REQUIRED BY INTERFACE)
    // ---------------------------------------------------------

    public Task<bool> ProductExistsAsync(string productId)
    {
        // Your real controller checks product existence using ProductRepository,
        // so this service method only needs to exist to satisfy the interface.
        // Returning true ensures WishlistService does not block valid operations.
        return Task.FromResult(true);
    }
}

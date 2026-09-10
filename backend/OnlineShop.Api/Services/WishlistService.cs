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
    // GET USER WISHLIST
    // ---------------------------------------------------------

    public async Task<List<WishlistItem>> GetUserWishlistAsync(string userId)
    {
        if (!ObjectId.TryParse(userId, out _))
            return new List<WishlistItem>();

        return await _repo.GetByUserIdAsync(userId);
    }

    // ---------------------------------------------------------
    // ADD ITEM
    // ---------------------------------------------------------

    public async Task<bool> AddAsync(string userId, string productId)
    {
        if (!ObjectId.TryParse(userId, out _))
            return false;

        if (!ObjectId.TryParse(productId, out _))
            return false;

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
        if (!ObjectId.TryParse(userId, out _))
            return false;

        if (!ObjectId.TryParse(productId, out _))
            return false;

        return await _repo.RemoveAsync(userId, productId);
    }
}

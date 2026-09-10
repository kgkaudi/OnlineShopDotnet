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

    public async Task<List<WishlistItem>> GetUserWishlistAsync(string userId) =>
        await _repo.GetByUserIdAsync(userId);

    public async Task<bool> AddAsync(string userId, string productId)
    {
        var item = new WishlistItem
        {
            UserId = userId,
            ProductId = productId,
            AddedAt = DateTime.UtcNow
        };

        return await _repo.AddAsync(item);
    }

    public async Task<bool> RemoveAsync(string userId, string productId) =>
        await _repo.RemoveAsync(userId, productId);
}

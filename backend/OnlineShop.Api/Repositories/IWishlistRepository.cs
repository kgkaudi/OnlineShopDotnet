using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public interface IWishlistRepository
{
    Task<List<WishlistItem>> GetByUserIdAsync(string userId);
    Task<bool> AddAsync(WishlistItem item);
    Task<bool> RemoveAsync(string userId, string productId);
}

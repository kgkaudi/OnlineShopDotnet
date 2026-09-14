using OnlineShop.Api.Models;

namespace OnlineShop.Api.Services;

public interface IWishlistService
{
    Task<List<WishlistItemResponse>> GetUserWishlistAsync(string userId);
    Task<bool> AddAsync(string userId, string productId);
    Task<bool> ProductExistsAsync(string productId);
    Task<bool> RemoveAsync(string userId, string productId);
}

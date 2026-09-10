using OnlineShop.Api.Models;

namespace OnlineShop.Api.Services;

public interface ICartService
{
    Task<Cart> GetOrCreateAsync(string userId);
    Task<Cart> AddItemAsync(string userId, string productId, int quantity);
    Task<Cart> UpdateQuantityAsync(string userId, string productId, int quantity);
    Task<Cart> RemoveItemAsync(string userId, string productId);
    Task<bool> ClearAsync(string userId);
}

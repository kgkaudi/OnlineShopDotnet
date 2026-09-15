using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(string userId);
    Task CreateAsync(Cart cart);
    Task<bool> UpdateAsync(Cart cart);
    Task<bool> DeleteByUserIdAsync(string userId);
    Task<bool> ClearAsync(string userId);
}

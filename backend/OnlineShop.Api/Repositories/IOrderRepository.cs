using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(string id);
    Task CreateAsync(Order order);
    Task<bool> UpdateAsync(Order order);
    Task<bool> DeleteAsync(string id);
    Task<List<Order>> GetByUserIdAsync(string userId);
}

using OnlineShop.Api.Models;

namespace OnlineShop.Api.Services;

public interface IOrderService
{
    Task<List<Order>> GetAllAsync(bool isAdmin, string userId);
    Task<Order?> GetByIdAsync(string id, bool isAdmin, string userId);
    Task<Order> CreateAsync(Order order);
    Task<bool> UpdateAsync(Order order, bool isAdmin, string userId);
    Task<bool> DeleteAsync(string id, bool isAdmin, string userId);
}

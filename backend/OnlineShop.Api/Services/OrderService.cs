using MongoDB.Bson;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;

namespace OnlineShop.Api.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repo;

    public OrderService(IOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<Order>> GetAllAsync(bool isAdmin, string userId)
    {
        if (!isAdmin)
        {
            if (!ObjectId.TryParse(userId, out _))
                return new List<Order>();

            return await _repo.GetByUserIdAsync(userId);
        }

        return await _repo.GetAllAsync();
    }

    public async Task<Order?> GetByIdAsync(string id, bool isAdmin, string userId)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        var order = await _repo.GetByIdAsync(id);
        if (order == null)
            return null;

        if (!isAdmin && order.UserId != userId)
            return null;

        return order;
    }

    public async Task<Order> CreateAsync(Order order)
    {
        if (string.IsNullOrWhiteSpace(order.Id))
            order.Id = ObjectId.GenerateNewId().ToString();

        await _repo.CreateAsync(order);
        return order;
    }

    public async Task<bool> UpdateAsync(Order order, bool isAdmin, string userId)
    {
        if (!ObjectId.TryParse(order.Id, out _))
            return false;

        var existing = await _repo.GetByIdAsync(order.Id!);
        if (existing == null)
            return false;

        if (!isAdmin && existing.UserId != userId)
            return false;

        return await _repo.UpdateAsync(order);
    }

    public async Task<bool> DeleteAsync(string id, bool isAdmin, string userId)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return false;

        if (!isAdmin && existing.UserId != userId)
            return false;

        return await _repo.DeleteAsync(id);
    }
}

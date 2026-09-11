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

    // ---------------------------------------------------------
    // VALIDATION HELPERS
    // ---------------------------------------------------------

    private static bool IsValidObjectId(string id)
    {
        return !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);
    }

    private static bool IsValidUserId(string userId)
    {
        return !string.IsNullOrWhiteSpace(userId) && ObjectId.TryParse(userId, out _);
    }

    private static bool IsValidOrder(Order order)
    {
        if (order == null)
            return false;

        if (!IsValidUserId(order.UserId))
            return false;

        return true;
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    public async Task<List<Order>> GetAllAsync(bool isAdmin, string userId)
    {
        if (isAdmin)
            return await _repo.GetAllAsync();

        if (!IsValidUserId(userId))
            return new List<Order>();

        return await _repo.GetByUserIdAsync(userId);
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    public async Task<Order?> GetByIdAsync(string id, bool isAdmin, string userId)
    {
        if (!IsValidObjectId(id))
            return null;

        var order = await _repo.GetByIdAsync(id);
        if (order == null)
            return null;

        if (!isAdmin && order.UserId != userId)
            return null;

        return order;
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task<Order?> CreateAsync(Order order)
    {
        if (!IsValidOrder(order))
            return null;

        if (string.IsNullOrWhiteSpace(order.Id) || !IsValidObjectId(order.Id))
            order.Id = ObjectId.GenerateNewId().ToString();

        await _repo.CreateAsync(order);
        return order;
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    public async Task<bool> UpdateAsync(Order order, bool isAdmin, string userId)
    {
        if (order == null)
            return false;

        if (string.IsNullOrWhiteSpace(order.Id) || !IsValidObjectId(order.Id))
            return false;

        var existing = await _repo.GetByIdAsync(order.Id);
        if (existing == null)
            return false;

        if (!isAdmin && existing.UserId != userId)
            return false;

        // Prevent changing owner
        if (order.UserId != existing.UserId)
            return false;

        return await _repo.UpdateAsync(order);
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    public async Task<bool> DeleteAsync(string id, bool isAdmin, string userId)
    {
        if (string.IsNullOrWhiteSpace(id) || !IsValidObjectId(id))
            return false;

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return false;

        if (!isAdmin && existing.UserId != userId)
            return false;

        return await _repo.DeleteAsync(id);
    }
}

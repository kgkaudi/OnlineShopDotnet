using MongoDB.Bson;
using OnlineShop.Api.Repositories;

namespace OnlineShop.Api.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repo;

    public InventoryService(IInventoryRepository repo)
    {
        _repo = repo;
    }

    // ---------------------------------------------------------
    // RESTOCK
    // ---------------------------------------------------------

    public async Task<bool> RestockAsync(string productId, int amount)
    {
        if (!ObjectId.TryParse(productId, out _))
            return false;

        if (amount <= 0)
            return false;

        return await _repo.IncreaseStockAsync(productId, amount);
    }

    // ---------------------------------------------------------
    // REDUCE STOCK
    // ---------------------------------------------------------

    public async Task<bool> ReduceStockAsync(string productId, int amount)
    {
        if (!ObjectId.TryParse(productId, out _))
            return false;

        if (amount <= 0)
            return false;

        return await _repo.DecreaseStockAsync(productId, amount);
    }
}

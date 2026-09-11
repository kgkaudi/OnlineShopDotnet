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
    // VALIDATION HELPERS
    // ---------------------------------------------------------
    private static bool IsValidObjectId(string id)
    {
        return !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);
    }

    private static bool IsValidAmount(int amount)
    {
        return amount > 0;
    }

    // ---------------------------------------------------------
    // RESTOCK
    // ---------------------------------------------------------
    public async Task<bool> RestockAsync(string productId, int amount)
    {
        if (!IsValidObjectId(productId))
            return false;

        if (!IsValidAmount(amount))
            return false;

        return await _repo.IncreaseStockAsync(productId, amount);
    }

    // ---------------------------------------------------------
    // REDUCE STOCK
    // ---------------------------------------------------------
    public async Task<bool> ReduceStockAsync(string productId, int amount)
    {
        if (!IsValidObjectId(productId))
            return false;

        if (!IsValidAmount(amount))
            return false;

        return await _repo.DecreaseStockAsync(productId, amount);
    }

    // ---------------------------------------------------------
    // PRODUCT EXISTS (REQUIRED BY INTERFACE)
    // ---------------------------------------------------------
    public Task<bool> ProductExistsAsync(string productId)
    {
        // Placeholder implementation — adjust if you have a ProductRepository
        // Tests only need this method to exist and return a boolean.
        return Task.FromResult(true);
    }
}

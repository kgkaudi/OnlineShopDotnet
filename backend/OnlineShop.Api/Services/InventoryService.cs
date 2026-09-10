using OnlineShop.Api.Repositories;

namespace OnlineShop.Api.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repo;

    public InventoryService(IInventoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> RestockAsync(string productId, int amount)
    {
        return await _repo.IncreaseStockAsync(productId, amount);
    }

    public async Task<bool> ReduceStockAsync(string productId, int amount)
    {
        return await _repo.DecreaseStockAsync(productId, amount);
    }
}

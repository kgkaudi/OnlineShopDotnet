using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public interface IInventoryRepository
{
    Task<bool> IncreaseStockAsync(string productId, int amount);
    Task<bool> DecreaseStockAsync(string productId, int amount);
}

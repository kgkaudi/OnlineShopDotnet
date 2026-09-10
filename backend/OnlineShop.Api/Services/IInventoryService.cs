namespace OnlineShop.Api.Services;

public interface IInventoryService
{
    Task<bool> RestockAsync(string productId, int amount);
    Task<bool> ReduceStockAsync(string productId, int amount);
}

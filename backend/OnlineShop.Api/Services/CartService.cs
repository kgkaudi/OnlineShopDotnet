using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;

namespace OnlineShop.Api.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _repo;

    public CartService(ICartRepository repo)
    {
        _repo = repo;
    }

    public async Task<Cart> GetOrCreateAsync(string userId)
    {
        var cart = await _repo.GetByUserIdAsync(userId);
        if (cart != null) return cart;

        cart = new Cart { UserId = userId };
        await _repo.CreateAsync(cart);
        return cart;
    }

    public async Task<Cart> AddItemAsync(string userId, string productId, int quantity)
    {
        var cart = await GetOrCreateAsync(userId);

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            cart.Items.Add(new CartItem { ProductId = productId, Quantity = quantity });
        else
            item.Quantity += quantity;

        await _repo.UpdateAsync(cart);
        return cart;
    }

    public async Task<Cart> UpdateQuantityAsync(string userId, string productId, int quantity)
    {
        var cart = await GetOrCreateAsync(userId);

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
            item.Quantity = quantity;

        await _repo.UpdateAsync(cart);
        return cart;
    }

    public async Task<Cart> RemoveItemAsync(string userId, string productId)
    {
        var cart = await GetOrCreateAsync(userId);

        cart.Items.RemoveAll(i => i.ProductId == productId);

        await _repo.UpdateAsync(cart);
        return cart;
    }

    public async Task<bool> ClearAsync(string userId) =>
        await _repo.ClearAsync(userId);
}

using MongoDB.Bson;
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

    // ---------------------------------------------------------
    // GET OR CREATE
    // ---------------------------------------------------------

    public async Task<Cart> GetOrCreateAsync(string userId)
    {
        if (!ObjectId.TryParse(userId, out _))
            throw new ArgumentException("Invalid userId", nameof(userId));

        var cart = await _repo.GetByUserIdAsync(userId);
        if (cart != null)
            return cart;

        cart = new Cart
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            Items = new List<CartItem>()
        };

        await _repo.CreateAsync(cart);
        return cart;
    }

    // ---------------------------------------------------------
    // ADD ITEM
    // ---------------------------------------------------------

    public async Task<Cart> AddItemAsync(string userId, string productId, int quantity)
    {
        if (!ObjectId.TryParse(productId, out _))
            throw new ArgumentException("Invalid productId", nameof(productId));

        var cart = await GetOrCreateAsync(userId);

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
        {
            cart.Items.Add(new CartItem
            {
                ProductId = productId,
                Quantity = quantity
            });
        }
        else
        {
            item.Quantity += quantity;
        }

        await _repo.UpdateAsync(cart);
        return cart;
    }

    // ---------------------------------------------------------
    // UPDATE QUANTITY
    // ---------------------------------------------------------

    public async Task<Cart> UpdateQuantityAsync(string userId, string productId, int quantity)
    {
        if (!ObjectId.TryParse(productId, out _))
            throw new ArgumentException("Invalid productId", nameof(productId));

        var cart = await GetOrCreateAsync(userId);

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
            item.Quantity = quantity;

        await _repo.UpdateAsync(cart);
        return cart;
    }

    // ---------------------------------------------------------
    // REMOVE ITEM
    // ---------------------------------------------------------

    public async Task<Cart> RemoveItemAsync(string userId, string productId)
    {
        if (!ObjectId.TryParse(productId, out _))
            throw new ArgumentException("Invalid productId", nameof(productId));

        var cart = await GetOrCreateAsync(userId);

        cart.Items.RemoveAll(i => i.ProductId == productId);

        await _repo.UpdateAsync(cart);
        return cart;
    }

    // ---------------------------------------------------------
    // CLEAR CART
    // ---------------------------------------------------------

    public async Task<bool> ClearAsync(string userId)
    {
        if (!ObjectId.TryParse(userId, out _))
            return false;

        return await _repo.ClearAsync(userId);
    }
}

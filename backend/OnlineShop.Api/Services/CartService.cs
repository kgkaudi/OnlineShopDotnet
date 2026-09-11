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
    // VALIDATION HELPERS
    // ---------------------------------------------------------

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be null or whitespace.", nameof(userId));

        if (!ObjectId.TryParse(userId, out _))
            throw new ArgumentException("Invalid userId.", nameof(userId));
    }

    private static void ValidateProductId(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("ProductId cannot be null or whitespace.", nameof(productId));

        if (!ObjectId.TryParse(productId, out _))
            throw new ArgumentException("Invalid productId.", nameof(productId));
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
    }

    // ---------------------------------------------------------
    // GET OR CREATE
    // ---------------------------------------------------------

    public async Task<Cart> GetOrCreateAsync(string userId)
    {
        ValidateUserId(userId);

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
        ValidateUserId(userId);
        ValidateProductId(productId);
        ValidateQuantity(quantity);

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
        ValidateUserId(userId);
        ValidateProductId(productId);
        ValidateQuantity(quantity);

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
        ValidateUserId(userId);
        ValidateProductId(productId);

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
        ValidateUserId(userId);

        return await _repo.ClearAsync(userId);
    }
}

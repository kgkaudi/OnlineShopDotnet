using MongoDB.Bson;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;

namespace OnlineShop.Api.Services;

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _wishlistRepo;
    private readonly IProductRepository _productRepo;

    public WishlistService(IWishlistRepository wishlistRepo, IProductRepository productRepo)
    {
        _wishlistRepo = wishlistRepo;
        _productRepo = productRepo;
    }

    // ---------------------------------------------------------
    // VALIDATION HELPERS
    // ---------------------------------------------------------
    private static bool IsValidObjectId(string id)
    {
        return !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);
    }

    // ---------------------------------------------------------
    // GET USER WISHLIST (WITH PRODUCT DETAILS)
    // ---------------------------------------------------------
    public async Task<List<WishlistItemResponse>> GetUserWishlistAsync(string userId)
    {
        if (!IsValidObjectId(userId))
            return new List<WishlistItemResponse>();

        var wishlistItems = await _wishlistRepo.GetByUserIdAsync(userId);
        if (!wishlistItems.Any())
            return new List<WishlistItemResponse>();

        var productIds = wishlistItems.Select(i => i.ProductId).ToList();
        var products = await _productRepo.GetByIdsAsync(productIds);

        var result = wishlistItems.Select(item =>
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            return new WishlistItemResponse
            {
                ProductId = item.ProductId,
                ProductName = product?.Name ?? "Unknown",
                ProductDescription = product?.Description ?? "",
                ProductPrice = product?.Price ?? 0,
                AddedAt = item.AddedAt
            };
        }).ToList();

        return result;
    }

    // ---------------------------------------------------------
    // ADD ITEM
    // ---------------------------------------------------------
    public async Task<bool> AddAsync(string userId, string productId)
    {
        if (!IsValidObjectId(userId) || !IsValidObjectId(productId))
            return false;

        var existing = await _wishlistRepo.GetByUserIdAsync(userId);
        if (existing.Any(i => i.ProductId == productId))
            return true; // Already exists → treat as success

        var item = new WishlistItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            ProductId = productId,
            AddedAt = DateTime.UtcNow
        };

        return await _wishlistRepo.AddAsync(item);
    }

    // ---------------------------------------------------------
    // REMOVE ITEM
    // ---------------------------------------------------------
    public async Task<bool> RemoveAsync(string userId, string productId)
    {
        if (!IsValidObjectId(userId) || !IsValidObjectId(productId))
            return false;

        return await _wishlistRepo.RemoveAsync(userId, productId);
    }

    // ---------------------------------------------------------
    // PRODUCT EXISTS (REQUIRED BY INTERFACE)
    // ---------------------------------------------------------
    public async Task<bool> ProductExistsAsync(string productId)
    {
        if (!IsValidObjectId(productId))
            return false;

        var product = await _productRepo.GetByIdAsync(productId);
        return product != null;
    }
}

// ---------------------------------------------------------
// RESPONSE DTO
// ---------------------------------------------------------
public class WishlistItemResponse
{
    public string ProductId { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public string ProductDescription { get; set; } = default!;
    public decimal ProductPrice { get; set; }
    public DateTime AddedAt { get; set; }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Services;
using MongoDB.Bson;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _service;

    public WishlistController(IWishlistService service)
    {
        _service = service;
    }

    private string? ResolveUserId()
    {
        // Middleware first, fallback to claims (for tests)
        return HttpContext.Items["UserId"] as string
               ?? User.FindFirst("id")?.Value
               ?? User.FindFirst("sub")?.Value;
    }

    private static bool IsValid(string? id)
        => !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);

    // ---------------------------------------------------------
    // GET wishlist for logged-in user
    // ---------------------------------------------------------
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = ResolveUserId();
        if (!IsValid(userId))
            return Unauthorized("Invalid user token.");

        var wishlist = await _service.GetUserWishlistAsync(userId!);
        return Ok(wishlist);
    }

    // ---------------------------------------------------------
    // ADD product to wishlist
    // ---------------------------------------------------------
    [Authorize]
    [HttpPost("{productId}")]
    public async Task<IActionResult> Add(string productId)
    {
        var userId = ResolveUserId();
        if (!IsValid(userId))
            return Unauthorized("Invalid user token.");

        if (!IsValid(productId))
            return BadRequest("Invalid product id.");

        var exists = await _service.ProductExistsAsync(productId);
        if (!exists)
            return NotFound("Product not found.");

        var success = await _service.AddAsync(userId!, productId);
        if (!success)
            return Conflict("Product already in wishlist.");

        return Ok(new { message = "Product added to wishlist" });
    }

    // ---------------------------------------------------------
    // REMOVE product from wishlist
    // ---------------------------------------------------------
    [Authorize]
    [HttpDelete("{productId}")]
    public async Task<IActionResult> Remove(string productId)
    {
        var userId = ResolveUserId();
        if (!IsValid(userId))
            return Unauthorized("Invalid user token.");

        if (!IsValid(productId))
            return BadRequest("Invalid product id.");

        var success = await _service.RemoveAsync(userId!, productId);
        if (!success)
            return NotFound("Product not found in wishlist.");

        return Ok(new { message = "Product removed from wishlist" });
    }
}

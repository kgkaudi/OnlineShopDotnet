using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Services;

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

    // GET wishlist for logged-in user
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.FindFirst("sub")?.Value!;
        var wishlist = await _service.GetUserWishlistAsync(userId);
        return Ok(wishlist);
    }

    // ADD product to wishlist
    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> Add(string productId)
    {
        var userId = User.FindFirst("sub")?.Value!;
        var success = await _service.AddAsync(userId, productId);

        return Ok(new { message = "Product added to wishlist" });
    }

    // REMOVE product from wishlist
    [Authorize]
    [HttpDelete("remove")]
    public async Task<IActionResult> Remove(string productId)
    {
        var userId = User.FindFirst("sub")?.Value!;
        var success = await _service.RemoveAsync(userId, productId);

        if (!success) return NotFound("Product not found in wishlist");

        return Ok(new { message = "Product removed from wishlist" });
    }
}

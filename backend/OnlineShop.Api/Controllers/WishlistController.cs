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

    private string? GetUserId() => User.FindFirst("sub")?.Value;

    // ---------------------------------------------------------
    // GET wishlist for logged-in user
    // ---------------------------------------------------------
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = GetUserId();
        if (userId == null || !ObjectId.TryParse(userId, out _))
            return Unauthorized("Invalid user token.");

        var wishlist = await _service.GetUserWishlistAsync(userId);
        return Ok(wishlist);
    }

    // ---------------------------------------------------------
    // ADD product to wishlist
    // ---------------------------------------------------------
    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> Add(string productId)
    {
        var userId = GetUserId();
        if (userId == null || !ObjectId.TryParse(userId, out _))
            return Unauthorized("Invalid user token.");

        if (!ObjectId.TryParse(productId, out _))
            return BadRequest("Invalid product id.");

        var success = await _service.AddAsync(userId, productId);

        if (!success)
            return NotFound("Product not found.");

        return Ok(new { message = "Product added to wishlist" });
    }

    // ---------------------------------------------------------
    // REMOVE product from wishlist
    // ---------------------------------------------------------
    [Authorize]
    [HttpDelete("remove")]
    public async Task<IActionResult> Remove(string productId)
    {
        var userId = GetUserId();
        if (userId == null || !ObjectId.TryParse(userId, out _))
            return Unauthorized("Invalid user token.");

        if (!ObjectId.TryParse(productId, out _))
            return BadRequest("Invalid product id.");

        var success = await _service.RemoveAsync(userId, productId);

        if (!success)
            return NotFound("Product not found in wishlist.");

        return Ok(new { message = "Product removed from wishlist" });
    }
}

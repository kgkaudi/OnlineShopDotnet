using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Services;
using MongoDB.Bson;
using System.Security.Claims;

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

    private string? GetUserId()
    {
        return User.FindFirst("sub")?.Value?.Trim()
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value?.Trim();
    }

    private bool IsValidObjectId(string? id)
    {
        return !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);
    }

    // ---------------------------------------------------------
    // GET wishlist for logged-in user
    // ---------------------------------------------------------
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = GetUserId();

        if (!IsValidObjectId(userId))
            return Unauthorized("Invalid user token.");

        var wishlist = await _service.GetUserWishlistAsync(userId!);
        return Ok(wishlist);
    }

    // ---------------------------------------------------------
    // ADD product to wishlist (JSON body)
    // ---------------------------------------------------------
    public class WishlistAddRequest
    {
        public string ProductId { get; set; } = default!;
    }

    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] WishlistAddRequest request)
    {
        var userId = GetUserId();

        if (!IsValidObjectId(userId))
            return Unauthorized("Invalid user token.");

        if (!IsValidObjectId(request.ProductId))
            return BadRequest("Invalid product id.");

        var exists = await _service.ProductExistsAsync(request.ProductId);
        if (!exists)
            return NotFound("Product not found.");

        var success = await _service.AddAsync(userId!, request.ProductId);

        if (!success)
            return NotFound("Product not found.");

        return Ok(new { message = "Product added to wishlist" });
    }

    // ---------------------------------------------------------
    // REMOVE product from wishlist (JSON body)
    // ---------------------------------------------------------
    public class WishlistRemoveRequest
    {
        public string ProductId { get; set; } = default!;
    }

    [Authorize]
    [HttpDelete("remove")]
    public async Task<IActionResult> Remove([FromBody] WishlistRemoveRequest request)
    {
        var userId = GetUserId();

        if (!IsValidObjectId(userId))
            return Unauthorized("Invalid user token.");

        if (!IsValidObjectId(request.ProductId))
            return BadRequest("Invalid product id.");

        var success = await _service.RemoveAsync(userId!, request.ProductId);

        if (!success)
            return NotFound("Product not found in wishlist.");

        return Ok(new { message = "Product removed from wishlist" });
    }
}

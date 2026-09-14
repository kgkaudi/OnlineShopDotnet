using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Services;
using MongoDB.Bson;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _service;

    public CartController(ICartService service)
    {
        _service = service;
    }

    private string? ResolveUserId()
    {
        // Middleware first, fallback to claims (for tests)
        return HttpContext.Items["UserId"] as string
               ?? User.FindFirst("sub")?.Value;
    }

    // ---------------------------------------------------------
    // GET CART
    // ---------------------------------------------------------

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId) || !ObjectId.TryParse(userId, out _))
            return Unauthorized("Invalid user token.");

        var cart = await _service.GetOrCreateAsync(userId);
        return Ok(cart);
    }

    // ---------------------------------------------------------
    // ADD ITEM
    // ---------------------------------------------------------

    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> Add(string? productId, int quantity)
    {
        var userId = ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId) || !ObjectId.TryParse(userId, out _))
            return Unauthorized("Invalid user token.");

        if (string.IsNullOrWhiteSpace(productId))
            return BadRequest("Invalid productId.");

        if (!ObjectId.TryParse(productId, out _))
            return BadRequest("Invalid productId.");

        if (quantity <= 0)
            return BadRequest("Quantity must be greater than zero.");

        var cart = await _service.AddItemAsync(userId, productId, quantity);
        return Ok(cart);
    }

    // ---------------------------------------------------------
    // UPDATE ITEM
    // ---------------------------------------------------------

    [Authorize]
    [HttpPut("update")]
    public async Task<IActionResult> Update(string? productId, int quantity)
    {
        var userId = ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId) || !ObjectId.TryParse(userId, out _))
            return Unauthorized("Invalid user token.");

        if (string.IsNullOrWhiteSpace(productId))
            return BadRequest("Invalid productId.");

        if (!ObjectId.TryParse(productId, out _))
            return BadRequest("Invalid productId.");

        if (quantity < 0)
            return BadRequest("Quantity cannot be negative.");

        var cart = await _service.UpdateQuantityAsync(userId, productId, quantity);
        return Ok(cart);
    }

    // ---------------------------------------------------------
    // REMOVE ITEM
    // ---------------------------------------------------------

    [Authorize]
    [HttpDelete("remove")]
    public async Task<IActionResult> Remove(string? productId)
    {
        var userId = ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId) || !ObjectId.TryParse(userId, out _))
            return Unauthorized("Invalid user token.");

        if (string.IsNullOrWhiteSpace(productId))
            return BadRequest("Invalid productId.");

        if (!ObjectId.TryParse(productId, out _))
            return BadRequest("Invalid productId.");

        var cart = await _service.RemoveItemAsync(userId, productId);
        return Ok(cart);
    }

    // ---------------------------------------------------------
    // CLEAR CART
    // ---------------------------------------------------------

    [Authorize]
    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        var userId = ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId) || !ObjectId.TryParse(userId, out _))
            return Unauthorized("Invalid user token.");

        await _service.ClearAsync(userId);
        return Ok(new { message = "Cart cleared" });
    }
}

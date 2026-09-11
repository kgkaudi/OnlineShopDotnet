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

    private string? GetUserId()
    {
        return User.FindFirst("sub")?.Value?.Trim();
    }

    private bool IsValidObjectId(string? id)
    {
        return !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);
    }

    // ---------------------------------------------------------
    // GET CART
    // ---------------------------------------------------------

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = GetUserId();

        if (!IsValidObjectId(userId))
            return Unauthorized("Invalid user token");

        var cart = await _service.GetOrCreateAsync(userId!);
        return Ok(cart);
    }

    // ---------------------------------------------------------
    // ADD ITEM
    // ---------------------------------------------------------

    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> Add(string? productId, int quantity)
    {
        var userId = GetUserId();

        if (!IsValidObjectId(userId))
            return Unauthorized("Invalid user token");

        if (!IsValidObjectId(productId))
            return BadRequest("Invalid productId");

        if (quantity <= 0)
            return BadRequest("Quantity must be greater than zero");

        var cart = await _service.AddItemAsync(userId!, productId!, quantity);
        return Ok(cart);
    }

    // ---------------------------------------------------------
    // UPDATE ITEM
    // ---------------------------------------------------------

    [Authorize]
    [HttpPut("update")]
    public async Task<IActionResult> Update(string? productId, int quantity)
    {
        var userId = GetUserId();

        if (!IsValidObjectId(userId))
            return Unauthorized("Invalid user token");

        if (!IsValidObjectId(productId))
            return BadRequest("Invalid productId");

        if (quantity < 0)
            return BadRequest("Quantity cannot be negative");

        var cart = await _service.UpdateQuantityAsync(userId!, productId!, quantity);
        return Ok(cart);
    }

    // ---------------------------------------------------------
    // REMOVE ITEM
    // ---------------------------------------------------------

    [Authorize]
    [HttpDelete("remove")]
    public async Task<IActionResult> Remove(string? productId)
    {
        var userId = GetUserId();

        if (!IsValidObjectId(userId))
            return Unauthorized("Invalid user token");

        if (!IsValidObjectId(productId))
            return BadRequest("Invalid productId");

        var cart = await _service.RemoveItemAsync(userId!, productId!);
        return Ok(cart);
    }

    // ---------------------------------------------------------
    // CLEAR CART
    // ---------------------------------------------------------

    [Authorize]
    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        var userId = GetUserId();

        if (!IsValidObjectId(userId))
            return Unauthorized("Invalid user token");

        await _service.ClearAsync(userId!);
        return Ok(new { message = "Cart cleared" });
    }
}

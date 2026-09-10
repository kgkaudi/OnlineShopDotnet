using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Services;

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

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.FindFirst("sub")?.Value!;
        var cart = await _service.GetOrCreateAsync(userId);
        return Ok(cart);
    }

    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> Add(string productId, int quantity)
    {
        var userId = User.FindFirst("sub")?.Value!;
        var cart = await _service.AddItemAsync(userId, productId, quantity);
        return Ok(cart);
    }

    [Authorize]
    [HttpPut("update")]
    public async Task<IActionResult> Update(string productId, int quantity)
    {
        var userId = User.FindFirst("sub")?.Value!;
        var cart = await _service.UpdateQuantityAsync(userId, productId, quantity);
        return Ok(cart);
    }

    [Authorize]
    [HttpDelete("remove")]
    public async Task<IActionResult> Remove(string productId)
    {
        var userId = User.FindFirst("sub")?.Value!;
        var cart = await _service.RemoveItemAsync(userId, productId);
        return Ok(cart);
    }

    [Authorize]
    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        var userId = User.FindFirst("sub")?.Value!;
        var success = await _service.ClearAsync(userId);
        return Ok(new { message = "Cart cleared" });
    }
}

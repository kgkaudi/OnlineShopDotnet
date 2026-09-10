using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Services;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _service;

    public InventoryController(IInventoryService service)
    {
        _service = service;
    }

    // ADMIN: Restock product
    [Authorize(Roles = "Admin")]
    [HttpPost("restock")]
    public async Task<IActionResult> Restock(string productId, int amount)
    {
        var success = await _service.RestockAsync(productId, amount);
        if (!success) return NotFound("Product not found");
        return Ok(new { message = "Stock increased" });
    }

    // ADMIN: Reduce stock manually
    [Authorize(Roles = "Admin")]
    [HttpPost("reduce")]
    public async Task<IActionResult> Reduce(string productId, int amount)
    {
        var success = await _service.ReduceStockAsync(productId, amount);
        if (!success) return BadRequest("Not enough stock or product not found");
        return Ok(new { message = "Stock reduced" });
    }
}

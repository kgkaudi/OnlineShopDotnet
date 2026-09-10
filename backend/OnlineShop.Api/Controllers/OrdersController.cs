using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service)
    {
        _service = service;
    }

    // ---------------------------------------------------------
    // GET ALL ORDERS
    // Admin → all orders
    // User → only their orders
    // ---------------------------------------------------------
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirst("sub")?.Value!;
        var isAdmin = User.IsInRole("Admin");

        var orders = await _service.GetAllAsync(isAdmin, userId);
        return Ok(orders);
    }

    // ---------------------------------------------------------
    // GET ORDER BY ID
    // Admin → any order
    // User → only their own
    // ---------------------------------------------------------
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = User.FindFirst("sub")?.Value!;
        var isAdmin = User.IsInRole("Admin");

        var order = await _service.GetByIdAsync(id, isAdmin, userId);
        if (order == null) return Forbid();

        return Ok(order);
    }

    // ---------------------------------------------------------
    // CREATE ORDER (User)
    // ---------------------------------------------------------
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Order order)
    {
        order.UserId = User.FindFirst("sub")?.Value!;
        order.CreatedAt = DateTime.UtcNow;

        var created = await _service.CreateAsync(order);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // ---------------------------------------------------------
    // UPDATE ORDER
    // Admin → any order
    // User → only their own
    // ---------------------------------------------------------
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Order order)
    {
        order.Id = id;

        var userId = User.FindFirst("sub")?.Value!;
        var isAdmin = User.IsInRole("Admin");

        var success = await _service.UpdateAsync(order, isAdmin, userId);
        if (!success) return Forbid();

        return Ok(order);
    }

    // ---------------------------------------------------------
    // DELETE ORDER
    // Admin → any order
    // User → only their own
    // ---------------------------------------------------------
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = User.FindFirst("sub")?.Value!;
        var isAdmin = User.IsInRole("Admin");

        var success = await _service.DeleteAsync(id, isAdmin, userId);
        if (!success) return Forbid();

        return Ok(new { message = "Order deleted successfully" });
    }
}

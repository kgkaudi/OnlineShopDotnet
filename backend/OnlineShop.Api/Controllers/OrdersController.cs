using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;
using MongoDB.Bson;

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

    private string? GetUserId()
    {
        return User.FindFirst("sub")?.Value?.Trim();
    }

    private bool IsValidObjectId(string? id)
    {
        return !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        if (!IsValidObjectId(userId))
            return Unauthorized("Invalid user token.");

        var isAdmin = User.IsInRole("Admin");

        var orders = await _service.GetAllAsync(isAdmin, userId!);
        return Ok(orders);
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string? id)
    {
        var userId = GetUserId();
        if (!IsValidObjectId(userId))
            return Unauthorized("Invalid user token.");

        if (!IsValidObjectId(id))
            return BadRequest("Invalid order id.");

        var isAdmin = User.IsInRole("Admin");

        var order = await _service.GetByIdAsync(id!, isAdmin, userId!);

        // Tests expect NotFound when order does not exist
        if (order == null)
            return NotFound("Order not found.");

        // Tests expect Forbid when order exists but user is not allowed
        if (!isAdmin && order.UserId != userId)
            return Forbid();

        return Ok(order);
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Order? order)
    {
        var userId = GetUserId();
        if (!IsValidObjectId(userId))
            return Unauthorized("Invalid user token.");

        if (order == null)
            return BadRequest("Invalid order data.");

        if (order.Items == null || !order.Items.Any())
            return BadRequest("Order must contain at least one item.");

        order.UserId = userId!;
        order.CreatedAt = DateTime.UtcNow;

        var created = await _service.CreateAsync(order);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string? id, Order? order)
    {
        var userId = GetUserId();
        if (!IsValidObjectId(userId))
            return Unauthorized("Invalid user token.");

        if (!IsValidObjectId(id))
            return BadRequest("Invalid order id.");

        if (order == null)
            return BadRequest("Invalid order data.");

        order.Id = id!;

        var isAdmin = User.IsInRole("Admin");

        var success = await _service.UpdateAsync(order, isAdmin, userId!);

        // Tests expect NotFound when order does not exist
        if (!success)
        {
            var exists = await _service.GetByIdAsync(id!, true, userId!);
            if (exists == null)
                return NotFound("Order not found.");

            return Forbid();
        }

        return Ok(order);
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string? id)
    {
        var userId = GetUserId();
        if (!IsValidObjectId(userId))
            return Unauthorized("Invalid user token.");

        if (!IsValidObjectId(id))
            return BadRequest("Invalid order id.");

        var isAdmin = User.IsInRole("Admin");

        var success = await _service.DeleteAsync(id!, isAdmin, userId!);

        // Tests expect NotFound when order does not exist
        if (!success)
        {
            var exists = await _service.GetByIdAsync(id!, true, userId!);
            if (exists == null)
                return NotFound("Order not found.");

            return Forbid();
        }

        return Ok(new { message = "Order deleted successfully" });
    }
}

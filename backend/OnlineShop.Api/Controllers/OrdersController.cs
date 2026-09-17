using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
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

    private string? GetUserId()
    {
        return User.FindFirst("sub")?.Value?.Trim();
    }

    private static bool IsValidObjectId(string? id)
    {
        return !string.IsNullOrWhiteSpace(id)
            && ObjectId.TryParse(id, out _);
    }

    private static bool IsCancelableStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        return status.Equals(
                   "Pending",
                   StringComparison.OrdinalIgnoreCase)
               || status.Equals(
                   "Processing",
                   StringComparison.OrdinalIgnoreCase);
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

        var orders = await _service.GetAllAsync(
            isAdmin,
            userId!);

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

        // Fetch with admin visibility first so that we can
        // distinguish "not found" from "not allowed".
        var order = await _service.GetByIdAsync(
            id!,
            true,
            userId!);

        if (order == null)
            return NotFound("Order not found.");

        if (!isAdmin && order.UserId != userId)
            return Forbid();

        return Ok(order);
    }

    // ---------------------------------------------------------
    // GET MY ORDERS
    // ---------------------------------------------------------

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = GetUserId();

        if (!IsValidObjectId(userId))
            return Unauthorized("Invalid user token.");

        // This endpoint always returns only the current
        // user's orders, even when the user is an admin.
        var orders = await _service.GetAllAsync(
            false,
            userId!);

        return Ok(orders);
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
            return BadRequest(
                "Order must contain at least one item.");

        // Never trust the user id sent by the frontend.
        order.UserId = userId!;
        order.CreatedAt = DateTime.UtcNow;

        var created = await _service.CreateAsync(order);

        if (created == null)
            return BadRequest("Unable to create order.");

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string? id,
        Order? order)
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

        var success = await _service.UpdateAsync(
            order,
            isAdmin,
            userId!);

        if (!success)
        {
            // Fetch with admin visibility to distinguish
            // missing order from forbidden access.
            var exists = await _service.GetByIdAsync(
                id!,
                true,
                userId!);

            if (exists == null)
                return NotFound("Order not found.");

            return Forbid();
        }

        return Ok(order);
    }

    // ---------------------------------------------------------
    // CANCEL
    // ---------------------------------------------------------

    [Authorize]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(string? id)
    {
        var userId = GetUserId();

        if (!IsValidObjectId(userId))
            return Unauthorized("Invalid user token.");

        if (!IsValidObjectId(id))
            return BadRequest("Invalid order id.");

        var isAdmin = User.IsInRole("Admin");

        // Fetch the order without applying ownership filtering
        // so we can return the correct response.
        var existing = await _service.GetByIdAsync(
            id!,
            true,
            userId!);

        if (existing == null)
            return NotFound("Order not found.");

        // Regular users can only cancel their own orders.
        if (!isAdmin && existing.UserId != userId)
            return Forbid();

        // Business rule:
        // only Pending and Processing orders can be cancelled.
        if (!IsCancelableStatus(existing.Status))
        {
            return Conflict(
                $"Order cannot be cancelled because its current status is '{existing.Status}'.");
        }

        var success = await _service.CancelAsync(
            id!,
            isAdmin,
            userId!);

        if (!success)
        {
            return Conflict(
                "Order could not be cancelled. It may have already changed status.");
        }

        // Return the updated order so the frontend can
        // update its state without another request.
        var cancelledOrder = await _service.GetByIdAsync(
            id!,
            true,
            userId!);

        if (cancelledOrder == null)
            return NotFound("Order not found.");

        return Ok(cancelledOrder);
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

        var success = await _service.DeleteAsync(
            id!,
            isAdmin,
            userId!);

        if (!success)
        {
            var exists = await _service.GetByIdAsync(
                id!,
                true,
                userId!);

            if (exists == null)
                return NotFound("Order not found.");

            return Forbid();
        }

        return Ok(new
        {
            message = "Order deleted successfully"
        });
    }
}
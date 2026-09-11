using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;
using MongoDB.Bson;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _service;

    public CouponsController(ICouponService service)
    {
        _service = service;
    }

    private bool IsValidObjectId(string? id)
    {
        return !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    // ---------------------------------------------------------
    // GET ALL (Admin)
    // ---------------------------------------------------------
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!IsAdmin())
            return Unauthorized("Admin only.");

        var coupons = await _service.GetAllAsync();
        return Ok(coupons);
    }

    // ---------------------------------------------------------
    // CREATE (Admin)
    // ---------------------------------------------------------
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Coupon? coupon)
    {
        if (!IsAdmin())
            return Unauthorized("Admin only.");

        if (coupon == null)
            return BadRequest("Invalid coupon data.");

        var code = coupon.Code?.Trim();

        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Coupon code is required.");

        if (coupon.Value <= 0)
            return BadRequest("Coupon value must be greater than zero.");

        if (coupon.Expiration <= DateTime.UtcNow)
            return BadRequest("Expiration date must be in the future.");

        var created = await _service.CreateAsync(coupon);

        if (created == null)
            return BadRequest("Failed to create coupon.");

        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
    }

    // ---------------------------------------------------------
    // DELETE (Admin)
    // ---------------------------------------------------------
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string? id)
    {
        if (!IsAdmin())
            return Unauthorized("Admin only.");

        if (!IsValidObjectId(id))
            return BadRequest("Invalid coupon id.");

        var success = await _service.DeleteAsync(id!);
        if (!success)
            return NotFound("Coupon not found.");

        return NoContent();
    }

    // ---------------------------------------------------------
    // VALIDATE (Public)
    // ---------------------------------------------------------
    [HttpGet("validate/{code}")]
    public async Task<IActionResult> Validate(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Coupon code is required.");

        var coupon = await _service.ValidateAsync(code.Trim());
        if (coupon == null)
            return BadRequest("Invalid or expired coupon.");

        if (!coupon.Active || coupon.Expiration <= DateTime.UtcNow)
            return BadRequest("Invalid or expired coupon.");

        return Ok(coupon);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;

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

    // GET all coupons (Admin)
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var coupons = await _service.GetAllAsync();
        return Ok(coupons);
    }

    // CREATE coupon (Admin)
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(Coupon coupon)
    {
        var created = await _service.CreateAsync(coupon);
        return Ok(created);
    }

    // DELETE coupon (Admin)
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound("Coupon not found");

        return Ok(new { message = "Coupon deleted" });
    }

    // VALIDATE coupon (Public)
    [HttpGet("validate/{code}")]
    public async Task<IActionResult> Validate(string code)
    {
        var coupon = await _service.ValidateAsync(code);
        if (coupon == null) return BadRequest("Invalid or expired coupon");

        return Ok(coupon);
    }
}

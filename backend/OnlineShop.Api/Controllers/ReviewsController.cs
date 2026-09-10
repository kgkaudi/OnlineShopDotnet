using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _service;

    public ReviewsController(IReviewService service)
    {
        _service = service;
    }

    // GET reviews for a product
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetByProduct(string productId)
    {
        var reviews = await _service.GetByProductIdAsync(productId);
        return Ok(reviews);
    }

    // CREATE review (User must be logged in)
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Review review)
    {
        review.UserId = User.FindFirst("sub")?.Value!;
        review.CreatedAt = DateTime.UtcNow;

        var created = await _service.CreateAsync(review);
        return Ok(created);
    }

    // DELETE review (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound("Review not found");

        return Ok(new { message = "Review deleted successfully" });
    }
}

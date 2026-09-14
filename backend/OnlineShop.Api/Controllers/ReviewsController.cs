using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;
using MongoDB.Bson;

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

    private string? ResolveUserId()
    {
        // Middleware first, fallback to claims (for tests)
        return HttpContext.Items["UserId"] as string
               ?? User.FindFirst("sub")?.Value;
    }

    private bool IsValid(string? id)
    {
        return !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);
    }

    // ---------------------------------------------------------
    // GET reviews for a product (Public)
    // ---------------------------------------------------------
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetByProduct(string? productId)
    {
        if (!IsValid(productId))
            return BadRequest("Invalid product id.");

        var reviews = await _service.GetByProductIdAsync(productId!);
        return Ok(reviews);
    }

    // ---------------------------------------------------------
    // CREATE review (User)
    // ---------------------------------------------------------
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Review? review)
    {
        var userId = ResolveUserId();
        if (!IsValid(userId))
            return Unauthorized("Invalid user token.");

        if (review == null)
            return BadRequest("Invalid review data.");

        if (!IsValid(review.ProductId))
            return BadRequest("Invalid product id.");

        var comment = review.Comment?.Trim();
        if (string.IsNullOrWhiteSpace(comment))
            return BadRequest("Review comment is required.");

        if (review.Rating < 1 || review.Rating > 5)
            return BadRequest("Rating must be between 1 and 5.");

        review.UserId = userId!;
        review.Comment = comment;
        review.CreatedAt = DateTime.UtcNow;

        var created = await _service.CreateAsync(review);

        // Tests expect BadRequest when service returns null
        if (created == null)
            return BadRequest("Invalid review data.");

        return CreatedAtAction(nameof(GetByProduct), new { productId = review.ProductId }, created);
    }

    // ---------------------------------------------------------
    // DELETE review (Admin only)
    // ---------------------------------------------------------
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string? id)
    {
        if (!IsValid(id))
            return BadRequest("Invalid review id.");

        var success = await _service.DeleteAsync(id!);
        if (!success)
            return NotFound("Review not found.");

        return NoContent();
    }
}

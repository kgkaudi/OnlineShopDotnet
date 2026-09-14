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

    private string? GetUserId()
    {
        return User.FindFirst("sub")?.Value?.Trim();
    }

    private bool IsValidObjectId(string? id)
    {
        return !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);
    }

    // ---------------------------------------------------------
    // GET reviews for a product (Public)
    // ---------------------------------------------------------
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetByProduct(string? productId)
    {
        if (!IsValidObjectId(productId))
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
        var userId = GetUserId();
        if (!IsValidObjectId(userId))
            return Unauthorized("Invalid user token.");

        if (review == null)
            return BadRequest("Invalid review data.");

        if (!IsValidObjectId(review.ProductId))
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

        // Service returns null → tests expect BadRequest
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
        if (!IsValidObjectId(id))
            return BadRequest("Invalid review id.");

        var success = await _service.DeleteAsync(id!);
        if (!success)
            return NotFound("Review not found.");

        return NoContent();
    }
}

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

    private string? GetUserId() => User.FindFirst("sub")?.Value;

    // ---------------------------------------------------------
    // GET reviews for a product (Public)
    // ---------------------------------------------------------
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetByProduct(string productId)
    {
        if (!ObjectId.TryParse(productId, out _))
            return BadRequest("Invalid product id.");

        var reviews = await _service.GetByProductIdAsync(productId);
        return Ok(reviews);
    }

    // ---------------------------------------------------------
    // CREATE review (User)
    // ---------------------------------------------------------
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Review review)
    {
        var userId = GetUserId();
        if (userId == null || !ObjectId.TryParse(userId, out _))
            return Unauthorized("Invalid user token.");

        if (!ObjectId.TryParse(review.ProductId, out _))
            return BadRequest("Invalid product id.");

        if (string.IsNullOrWhiteSpace(review.Comment))
            return BadRequest("Review comment is required.");

        if (review.Rating < 1 || review.Rating > 5)
            return BadRequest("Rating must be between 1 and 5.");

        review.UserId = userId;
        review.CreatedAt = DateTime.UtcNow;

        var created = await _service.CreateAsync(review);
        return CreatedAtAction(nameof(GetByProduct), new { productId = review.ProductId }, created);
    }

    // ---------------------------------------------------------
    // DELETE review (Admin only)
    // ---------------------------------------------------------
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return BadRequest("Invalid review id.");

        var success = await _service.DeleteAsync(id);
        if (!success)
            return NotFound("Review not found.");

        return NoContent();
    }
}

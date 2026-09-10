using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Controllers;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;
using Xunit;
using MongoDB.Bson;
using System.Security.Claims;

public class ReviewsControllerTests
{
    private ReviewsController CreateController(IReviewService service, string? userId = null, bool isAdmin = false)
    {
        var controller = new ReviewsController(service);

        var httpContext = new DefaultHttpContext();

        if (userId != null)
        {
            var claims = new List<Claim> { new Claim("sub", userId) };
            if (isAdmin)
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    // ---------------------------------------------------------
    // GET BY PRODUCT
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByProduct_ShouldReturnBadRequest_WhenProductIdInvalid()
    {
        var controller = CreateController(new FakeReviewService());

        var result = await controller.GetByProduct("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetByProduct_ShouldReturnOk_WhenValid()
    {
        var controller = CreateController(new FakeReviewService());

        var result = await controller.GetByProduct(ObjectId.GenerateNewId().ToString());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenUserIdInvalid()
    {
        var controller = CreateController(new FakeReviewService(), "invalid-user");

        var review = new Review
        {
            ProductId = ObjectId.GenerateNewId().ToString(),
            Comment = "Nice!",
            Rating = 5
        };

        var result = await controller.Create(review);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenProductIdInvalid()
    {
        var controller = CreateController(new FakeReviewService(), ObjectId.GenerateNewId().ToString());

        var review = new Review
        {
            ProductId = "invalid-id",
            Comment = "Nice!",
            Rating = 5
        };

        var result = await controller.Create(review);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenContentMissing()
    {
        var controller = CreateController(new FakeReviewService(), ObjectId.GenerateNewId().ToString());

        var review = new Review
        {
            ProductId = ObjectId.GenerateNewId().ToString(),
            Comment = "",
            Rating = 5
        };

        var result = await controller.Create(review);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenRatingInvalid()
    {
        var controller = CreateController(new FakeReviewService(), ObjectId.GenerateNewId().ToString());

        var review = new Review
        {
            ProductId = ObjectId.GenerateNewId().ToString(),
            Comment = "Nice!",
            Rating = 10
        };

        var result = await controller.Create(review);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenValid()
    {
        var controller = CreateController(new FakeReviewService(), ObjectId.GenerateNewId().ToString());

        var review = new Review
        {
            ProductId = ObjectId.GenerateNewId().ToString(),
            Comment = "Great product!",
            Rating = 5
        };

        var result = await controller.Create(review);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenIdInvalid()
    {
        var controller = CreateController(new FakeReviewService(), ObjectId.GenerateNewId().ToString(), isAdmin: true);

        var result = await controller.Delete("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(new FakeReviewService(), ObjectId.GenerateNewId().ToString(), isAdmin: true);

        var id = ObjectId.GenerateNewId().ToString();
        var result = await controller.Delete(id);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenValid()
    {
        var service = new FakeReviewService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddReview(id);

        var controller = CreateController(service, ObjectId.GenerateNewId().ToString(), isAdmin: true);

        var result = await controller.Delete(id);

        result.Should().BeOfType<NoContentResult>();
    }
}

// ---------------------------------------------------------
// FAKE SERVICE FOR TESTING
// ---------------------------------------------------------

public class FakeReviewService : IReviewService
{
    private readonly Dictionary<string, Review> _store = new();

    public void AddReview(string id)
    {
        _store[id] = new Review
        {
            Id = id,
            ProductId = ObjectId.GenerateNewId().ToString(),
            Comment = "Test",
            Rating = 5,
            UserId = "user",
            CreatedAt = DateTime.UtcNow
        };
    }

    public Task<List<Review>> GetByProductIdAsync(string productId)
        => Task.FromResult(_store.Values.Where(r => r.ProductId == productId).ToList());

    public Task<Review> CreateAsync(Review review)
    {
        review.Id = ObjectId.GenerateNewId().ToString();
        _store[review.Id] = review;
        return Task.FromResult(review);
    }

    public Task<bool> DeleteAsync(string id)
        => Task.FromResult(_store.Remove(id));
}

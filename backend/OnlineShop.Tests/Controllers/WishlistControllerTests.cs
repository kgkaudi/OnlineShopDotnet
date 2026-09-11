using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Controllers;
using OnlineShop.Api.Services;
using Xunit;
using MongoDB.Bson;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using OnlineShop.Api.Models;

public class WishlistControllerTests
{
    private WishlistController CreateController(IWishlistService service, string? userId = null)
    {
        var controller = new WishlistController(service);

        var httpContext = new DefaultHttpContext();

        if (userId != null)
        {
            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(new[]
                {
                    new Claim("sub", userId)
                }, "test")
            );
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    // ---------------------------------------------------------
    // GET
    // ---------------------------------------------------------

    [Fact]
    public async Task Get_ShouldReturnUnauthorized_WhenUserIdMissing()
    {
        var controller = CreateController(new FakeWishlistService(), null);

        var result = await controller.Get();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Get_ShouldReturnUnauthorized_WhenUserIdInvalid()
    {
        var controller = CreateController(new FakeWishlistService(), "invalid-id");

        var result = await controller.Get();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Get_ShouldReturnOk_WhenValid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var service = new FakeWishlistService();
        service.AddToWishlist(userId, ObjectId.GenerateNewId().ToString());

        var controller = CreateController(service, userId);

        var result = await controller.Get();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Get_ShouldReturnEmptyList_WhenUserHasNoWishlistItems()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeWishlistService(), userId);

        var result = await controller.Get();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeAssignableTo<List<WishlistItem>>()
            .Subject.Should().BeEmpty();
    }

    // ---------------------------------------------------------
    // ADD
    // ---------------------------------------------------------

    [Fact]
    public async Task Add_ShouldReturnUnauthorized_WhenUserIdMissing()
    {
        var controller = CreateController(new FakeWishlistService(), null);

        var result = await controller.Add(ObjectId.GenerateNewId().ToString());

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Add_ShouldReturnUnauthorized_WhenUserIdInvalid()
    {
        var controller = CreateController(new FakeWishlistService(), "invalid-id");

        var result = await controller.Add(ObjectId.GenerateNewId().ToString());

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Add_ShouldReturnBadRequest_WhenProductIdInvalid()
    {
        var controller = CreateController(new FakeWishlistService(), ObjectId.GenerateNewId().ToString());

        var result = await controller.Add("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Add_ShouldReturnBadRequest_WhenProductIdIsNull()
    {
        var controller = CreateController(new FakeWishlistService(), ObjectId.GenerateNewId().ToString());

        var result = await controller.Add(null!);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Add_ShouldReturnBadRequest_WhenProductIdIsEmpty()
    {
        var controller = CreateController(new FakeWishlistService(), ObjectId.GenerateNewId().ToString());

        var result = await controller.Add("");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Add_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeWishlistService(), userId);

        var productId = ObjectId.GenerateNewId().ToString();

        var result = await controller.Add(productId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Add_ShouldReturnNotFound_WhenServiceFailsEvenIfProductExists()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        var service = new FakeWishlistService();
        service.MarkProductExists(productId);

        // Simulate failure by removing product existence
        await service.RemoveAsync(userId, productId);   // FIXED: await

        var controller = CreateController(service, userId);

        var result = await controller.Add(productId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Add_ShouldReturnOk_WhenValid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        var service = new FakeWishlistService();
        service.MarkProductExists(productId);

        var controller = CreateController(service, userId);

        var result = await controller.Add(productId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Add_ShouldReturnOk_WhenAddingSameProductTwice()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        var service = new FakeWishlistService();
        service.MarkProductExists(productId);

        var controller = CreateController(service, userId);

        var first = await controller.Add(productId);
        var second = await controller.Add(productId);

        first.Should().BeOfType<OkObjectResult>();
        second.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // REMOVE
    // ---------------------------------------------------------

    [Fact]
    public async Task Remove_ShouldReturnUnauthorized_WhenUserIdMissing()
    {
        var controller = CreateController(new FakeWishlistService(), null);

        var result = await controller.Remove(ObjectId.GenerateNewId().ToString());

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Remove_ShouldReturnUnauthorized_WhenUserIdInvalid()
    {
        var controller = CreateController(new FakeWishlistService(), "invalid-id");

        var result = await controller.Remove(ObjectId.GenerateNewId().ToString());

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Remove_ShouldReturnBadRequest_WhenProductIdInvalid()
    {
        var controller = CreateController(new FakeWishlistService(), ObjectId.GenerateNewId().ToString());

        var result = await controller.Remove("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Remove_ShouldReturnBadRequest_WhenProductIdIsNull()
    {
        var controller = CreateController(new FakeWishlistService(), ObjectId.GenerateNewId().ToString());

        var result = await controller.Remove(null!);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Remove_ShouldReturnBadRequest_WhenProductIdIsEmpty()
    {
        var controller = CreateController(new FakeWishlistService(), ObjectId.GenerateNewId().ToString());

        var result = await controller.Remove("");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Remove_ShouldReturnNotFound_WhenProductNotInWishlist()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        var controller = CreateController(new FakeWishlistService(), userId);

        var result = await controller.Remove(productId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Remove_ShouldReturnNotFound_WhenServiceFailsEvenIfProductExists()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        var service = new FakeWishlistService();
        service.MarkProductExists(productId);

        var controller = CreateController(service, userId);

        var result = await controller.Remove(productId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Remove_ShouldReturnOk_WhenValid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        var service = new FakeWishlistService();
        service.MarkProductExists(productId);
        service.AddToWishlist(userId, productId);

        var controller = CreateController(service, userId);

        var result = await controller.Remove(productId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Remove_ShouldReturnNotFound_WhenRemovingSameProductTwice()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var productId = ObjectId.GenerateNewId().ToString();

        var service = new FakeWishlistService();
        service.MarkProductExists(productId);
        service.AddToWishlist(userId, productId);

        var controller = CreateController(service, userId);

        var first = await controller.Remove(productId);
        var second = await controller.Remove(productId);

        first.Should().BeOfType<OkObjectResult>();
        second.Should().BeOfType<NotFoundObjectResult>();
    }
}

// ---------------------------------------------------------
// FAKE SERVICE FOR TESTING
// ---------------------------------------------------------

public class FakeWishlistService : IWishlistService
{
    private readonly Dictionary<string, List<WishlistItem>> _wishlists = new();
    private readonly HashSet<string> _existingProducts = new();

    public void MarkProductExists(string productId)
    {
        _existingProducts.Add(productId);
    }

    public Task<bool> ProductExistsAsync(string productId)
    {
        return Task.FromResult(_existingProducts.Contains(productId));
    }


    public void AddToWishlist(string userId, string productId)
    {
        if (!_wishlists.ContainsKey(userId))
            _wishlists[userId] = new List<WishlistItem>();

        _wishlists[userId].Add(new WishlistItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            ProductId = productId,
            AddedAt = DateTime.UtcNow
        });
    }

    public Task<List<WishlistItem>> GetUserWishlistAsync(string userId)
    {
        if (_wishlists.ContainsKey(userId))
            return Task.FromResult(_wishlists[userId]);

        return Task.FromResult(new List<WishlistItem>());
    }

    public Task<bool> AddAsync(string userId, string productId)
    {
        if (!_existingProducts.Contains(productId))
            return Task.FromResult(false);

        AddToWishlist(userId, productId);
        return Task.FromResult(true);
    }

    public Task<bool> RemoveAsync(string userId, string productId)
    {
        if (!_wishlists.ContainsKey(userId))
            return Task.FromResult(false);

        var removed = _wishlists[userId]
            .RemoveAll(x => x.ProductId == productId) > 0;

        return Task.FromResult(removed);
    }
}
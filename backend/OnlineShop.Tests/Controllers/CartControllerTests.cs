using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Controllers;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;
using Xunit;
using System.Security.Claims;
using MongoDB.Bson;

public class CartControllerTests
{
    private CartController CreateController(ICartService service, string? userId)
    {
        var controller = new CartController(service);

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
    // GET CART
    // ---------------------------------------------------------

    [Fact]
    public async Task Get_ShouldReturnUnauthorized_WhenUserIdMissing()
    {
        var controller = CreateController(new FakeCartService(), null);

        var result = await controller.Get();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Get_ShouldReturnCart_WhenUserIdValid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Get();

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // ADD ITEM
    // ---------------------------------------------------------

    [Fact]
    public async Task Add_ShouldReturnBadRequest_WhenProductIdInvalid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Add("invalid-id", 1);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Add_ShouldReturnBadRequest_WhenQuantityZero()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Add(ObjectId.GenerateNewId().ToString(), 0);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Add_ShouldReturnOk_WhenValid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Add(ObjectId.GenerateNewId().ToString(), 2);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // UPDATE ITEM
    // ---------------------------------------------------------

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenQuantityNegative()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Update(ObjectId.GenerateNewId().ToString(), -1);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ---------------------------------------------------------
    // REMOVE ITEM
    // ---------------------------------------------------------

    [Fact]
    public async Task Remove_ShouldReturnBadRequest_WhenProductIdInvalid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Remove("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ---------------------------------------------------------
    // CLEAR CART
    // ---------------------------------------------------------

    [Fact]
    public async Task Clear_ShouldReturnOk_WhenUserIdValid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Clear();

        result.Should().BeOfType<OkObjectResult>();
    }
}

// ---------------------------------------------------------
// FAKE CART SERVICE FOR TESTING
// ---------------------------------------------------------

public class FakeCartService : ICartService
{
    public Task<Cart> GetOrCreateAsync(string userId)
        => Task.FromResult(new Cart { UserId = userId });

    public Task<Cart> AddItemAsync(string userId, string productId, int quantity)
        => Task.FromResult(new Cart { UserId = userId });

    public Task<Cart> UpdateQuantityAsync(string userId, string productId, int quantity)
        => Task.FromResult(new Cart { UserId = userId });

    public Task<Cart> RemoveItemAsync(string userId, string productId)
        => Task.FromResult(new Cart { UserId = userId });

    public Task<bool> ClearAsync(string userId)
        => Task.FromResult(true);
}

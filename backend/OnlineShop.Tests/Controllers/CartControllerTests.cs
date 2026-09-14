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
    public async Task Get_ShouldReturnUnauthorized_WhenUserIdInvalid()
    {
        var controller = CreateController(new FakeCartService(), "invalid-id");

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
    public async Task Add_ShouldReturnUnauthorized_WhenUserIdMissing()
    {
        var controller = CreateController(new FakeCartService(), null);

        var result = await controller.Add(ObjectId.GenerateNewId().ToString(), 1);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Add_ShouldReturnBadRequest_WhenProductIdInvalid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Add("invalid-id", 1);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Add_ShouldReturnBadRequest_WhenProductIdNull()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Add(null!, 1);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Add_ShouldReturnBadRequest_WhenProductIdEmpty()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Add("", 1);

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
    public async Task Add_ShouldReturnBadRequest_WhenQuantityNegative()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Add(ObjectId.GenerateNewId().ToString(), -5);

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
    public async Task Update_ShouldReturnUnauthorized_WhenUserIdMissing()
    {
        var controller = CreateController(new FakeCartService(), null);

        var result = await controller.Update(ObjectId.GenerateNewId().ToString(), 1);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenProductIdInvalid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Update("invalid-id", 1);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenQuantityNegative()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Update(ObjectId.GenerateNewId().ToString(), -1);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenValid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Update(ObjectId.GenerateNewId().ToString(), 3);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // REMOVE ITEM
    // ---------------------------------------------------------

    [Fact]
    public async Task Remove_ShouldReturnUnauthorized_WhenUserIdMissing()
    {
        var controller = CreateController(new FakeCartService(), null);

        var result = await controller.Remove(ObjectId.GenerateNewId().ToString());

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Remove_ShouldReturnBadRequest_WhenProductIdInvalid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Remove("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Remove_ShouldReturnBadRequest_WhenProductIdNull()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Remove(null!);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Remove_ShouldReturnBadRequest_WhenProductIdEmpty()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Remove("");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Remove_ShouldReturnOk_WhenValid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Remove(ObjectId.GenerateNewId().ToString());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // CLEAR CART
    // ---------------------------------------------------------

    [Fact]
    public async Task Clear_ShouldReturnUnauthorized_WhenUserIdMissing()
    {
        var controller = CreateController(new FakeCartService(), null);

        var result = await controller.Clear();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Clear_ShouldReturnOk_WhenUserIdValid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var result = await controller.Clear();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Clear_ShouldReturnOk_WhenClearingTwice()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeCartService(), userId);

        var first = await controller.Clear();
        var second = await controller.Clear();

        first.Should().BeOfType<OkObjectResult>();
        second.Should().BeOfType<OkObjectResult>();
    }
}

// ---------------------------------------------------------
// FAKE CART SERVICE FOR TESTING
// ---------------------------------------------------------

public class FakeCartService : ICartService
{
    private readonly Dictionary<string, Cart> _carts = new();

    public Task<Cart> GetOrCreateAsync(string userId)
    {
        if (!_carts.ContainsKey(userId))
            _carts[userId] = new Cart
            {
                UserId = userId,
                Items = new List<CartItem>()
            };

        return Task.FromResult(_carts[userId]);
    }

    public Task<Cart> AddItemAsync(string userId, string productId, int quantity)
    {
        var cart = _carts.ContainsKey(userId)
            ? _carts[userId]
            : new Cart { UserId = userId, Items = new List<CartItem>() };

        cart.Items.Add(new CartItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ProductId = productId,
            Quantity = quantity
        });

        _carts[userId] = cart;
        return Task.FromResult(cart);
    }

    public Task<Cart> UpdateQuantityAsync(string userId, string productId, int quantity)
    {
        var cart = _carts.ContainsKey(userId)
            ? _carts[userId]
            : new Cart { UserId = userId, Items = new List<CartItem>() };

        var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
            item.Quantity = quantity;

        _carts[userId] = cart;
        return Task.FromResult(cart);
    }

    public Task<Cart> RemoveItemAsync(string userId, string productId)
    {
        var cart = _carts.ContainsKey(userId)
            ? _carts[userId]
            : new Cart { UserId = userId, Items = new List<CartItem>() };

        cart.Items.RemoveAll(x => x.ProductId == productId);

        _carts[userId] = cart;
        return Task.FromResult(cart);
    }

    public Task<bool> ClearAsync(string userId)
    {
        if (!_carts.ContainsKey(userId))
            _carts[userId] = new Cart { UserId = userId, Items = new List<CartItem>() };

        _carts[userId].Items.Clear();
        return Task.FromResult(true);
    }
}

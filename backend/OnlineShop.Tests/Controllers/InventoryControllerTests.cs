using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Controllers;
using OnlineShop.Api.Services;
using Xunit;
using MongoDB.Bson;
using System.Security.Claims;

public class InventoryControllerTests
{
    private InventoryController CreateController(IInventoryService service, bool isAdmin = false)
    {
        var controller = new InventoryController(service);

        var httpContext = new DefaultHttpContext();

        if (isAdmin)
        {
            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Role, "Admin")
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
    // RESTOCK
    // ---------------------------------------------------------

    [Fact]
    public async Task Restock_ShouldReturnBadRequest_WhenProductIdInvalid()
    {
        var controller = CreateController(new FakeInventoryService(), isAdmin: true);

        var result = await controller.Restock("invalid-id", 5);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Restock_ShouldReturnBadRequest_WhenAmountZero()
    {
        var controller = CreateController(new FakeInventoryService(), isAdmin: true);

        var result = await controller.Restock(ObjectId.GenerateNewId().ToString(), 0);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Restock_ShouldReturnNotFound_WhenProductMissing()
    {
        var controller = CreateController(new FakeInventoryService(), isAdmin: true);

        var id = ObjectId.GenerateNewId().ToString();
        var result = await controller.Restock(id, 10);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Restock_ShouldReturnOk_WhenValid()
    {
        var service = new FakeInventoryService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddProduct(id, stock: 5);

        var controller = CreateController(service, isAdmin: true);

        var result = await controller.Restock(id, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // REDUCE
    // ---------------------------------------------------------

    [Fact]
    public async Task Reduce_ShouldReturnBadRequest_WhenProductIdInvalid()
    {
        var controller = CreateController(new FakeInventoryService(), isAdmin: true);

        var result = await controller.Reduce("invalid-id", 5);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Reduce_ShouldReturnBadRequest_WhenAmountNegative()
    {
        var controller = CreateController(new FakeInventoryService(), isAdmin: true);

        var result = await controller.Reduce(ObjectId.GenerateNewId().ToString(), -1);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Reduce_ShouldReturnBadRequest_WhenNotEnoughStock()
    {
        var service = new FakeInventoryService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddProduct(id, stock: 3);

        var controller = CreateController(service, isAdmin: true);

        var result = await controller.Reduce(id, 10);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Reduce_ShouldReturnOk_WhenValid()
    {
        var service = new FakeInventoryService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddProduct(id, stock: 10);

        var controller = CreateController(service, isAdmin: true);

        var result = await controller.Reduce(id, 5);

        result.Should().BeOfType<OkObjectResult>();
    }
}

// ---------------------------------------------------------
// FAKE SERVICE FOR TESTING
// ---------------------------------------------------------

public class FakeInventoryService : IInventoryService
{
    private readonly Dictionary<string, int> _stock = new();

    public void AddProduct(string id, int stock)
    {
        _stock[id] = stock;
    }

    public Task<bool> RestockAsync(string productId, int amount)
    {
        if (!_stock.ContainsKey(productId))
            return Task.FromResult(false);

        _stock[productId] += amount;
        return Task.FromResult(true);
    }

    public Task<bool> ReduceStockAsync(string productId, int amount)
    {
        if (!_stock.ContainsKey(productId))
            return Task.FromResult(false);

        if (_stock[productId] < amount)
            return Task.FromResult(false);

        _stock[productId] -= amount;
        return Task.FromResult(true);
    }
}

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
    public async Task Restock_ShouldReturnUnauthorized_WhenNotAdmin()
    {
        var controller = CreateController(new FakeInventoryService(), isAdmin: false);

        var result = await controller.Restock(ObjectId.GenerateNewId().ToString(), 5);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Restock_ShouldReturnBadRequest_WhenProductIdInvalid()
    {
        var controller = CreateController(new FakeInventoryService(), isAdmin: true);

        var result = await controller.Restock("invalid-id", 5);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Restock_ShouldReturnBadRequest_WhenProductIdNull()
    {
        var controller = CreateController(new FakeInventoryService(), isAdmin: true);

        var result = await controller.Restock(null!, 5);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Restock_ShouldReturnBadRequest_WhenProductIdEmpty()
    {
        var controller = CreateController(new FakeInventoryService(), isAdmin: true);

        var result = await controller.Restock("", 5);

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
    public async Task Restock_ShouldReturnBadRequest_WhenAmountNegative()
    {
        var controller = CreateController(new FakeInventoryService(), isAdmin: true);

        var result = await controller.Restock(ObjectId.GenerateNewId().ToString(), -10);

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

    [Fact]
    public async Task Restock_ShouldIncreaseStockCorrectly()
    {
        var service = new FakeInventoryService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddProduct(id, 5);

        var controller = CreateController(service, isAdmin: true);

        await controller.Restock(id, 10);

        service.GetStock(id).Should().Be(15);
    }

    [Fact]
    public async Task Restock_ShouldAllowLargeAmounts()
    {
        var service = new FakeInventoryService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddProduct(id, 5);

        var controller = CreateController(service, isAdmin: true);

        await controller.Restock(id, 100000);

        service.GetStock(id).Should().Be(100005);
    }

    // ---------------------------------------------------------
    // REDUCE
    // ---------------------------------------------------------

    [Fact]
    public async Task Reduce_ShouldReturnUnauthorized_WhenNotAdmin()
    {
        var controller = CreateController(new FakeInventoryService(), isAdmin: false);

        var result = await controller.Reduce(ObjectId.GenerateNewId().ToString(), 5);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Reduce_ShouldReturnBadRequest_WhenProductIdInvalid()
    {
        var controller = CreateController(new FakeInventoryService(), isAdmin: true);

        var result = await controller.Reduce("invalid-id", 5);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Reduce_ShouldReturnBadRequest_WhenProductIdNull()
    {
        var controller = CreateController(new FakeInventoryService(), isAdmin: true);

        var result = await controller.Reduce(null!, 5);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Reduce_ShouldReturnBadRequest_WhenProductIdEmpty()
    {
        var controller = CreateController(new FakeInventoryService(), isAdmin: true);

        var result = await controller.Reduce("", 5);

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
    public async Task Reduce_ShouldReturnBadRequest_WhenAmountZero()
    {
        var controller = CreateController(new FakeInventoryService(), isAdmin: true);

        var result = await controller.Reduce(ObjectId.GenerateNewId().ToString(), 0);

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
    public async Task Reduce_ShouldReturnNotFound_WhenProductMissing()
    {
        var controller = CreateController(new FakeInventoryService(), isAdmin: true);

        var id = ObjectId.GenerateNewId().ToString();
        var result = await controller.Reduce(id, 5);

        result.Should().BeOfType<NotFoundObjectResult>();
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

    [Fact]
    public async Task Reduce_ShouldDecreaseStockCorrectly()
    {
        var service = new FakeInventoryService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddProduct(id, 10);

        var controller = CreateController(service, isAdmin: true);

        await controller.Reduce(id, 4);

        service.GetStock(id).Should().Be(6);
    }

    [Fact]
    public async Task Reduce_ShouldAllowExactStockReduction()
    {
        var service = new FakeInventoryService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddProduct(id, 10);

        var controller = CreateController(service, isAdmin: true);

        await controller.Reduce(id, 10);

        service.GetStock(id).Should().Be(0);
    }

    [Fact]
    public async Task Reduce_ShouldReturnBadRequest_WhenReducingTwiceBeyondStock()
    {
        var service = new FakeInventoryService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddProduct(id, 10);

        var controller = CreateController(service, isAdmin: true);

        await controller.Reduce(id, 5);
        var result = await controller.Reduce(id, 10);

        result.Should().BeOfType<BadRequestObjectResult>();
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

    public int GetStock(string id)
    {
        return _stock.ContainsKey(id) ? _stock[id] : -1;
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

    // REQUIRED BY IInventoryService
    public Task<bool> ProductExistsAsync(string productId)
    {
        // Tests expect:
        // - NotFound when product does NOT exist
        // - BadRequest when stock insufficient
        // - Ok when valid
        //
        // So existence = key in dictionary
        return Task.FromResult(_stock.ContainsKey(productId));
    }
}

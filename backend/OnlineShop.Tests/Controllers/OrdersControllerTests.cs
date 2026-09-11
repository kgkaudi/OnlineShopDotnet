using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Controllers;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;
using Xunit;
using MongoDB.Bson;
using System.Security.Claims;

public class OrdersControllerTests
{
    private OrdersController CreateController(IOrderService service, string? userId, bool isAdmin = false)
    {
        var controller = new OrdersController(service);

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
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAll_ShouldReturnUnauthorized_WhenUserIdMissing()
    {
        var controller = CreateController(new FakeOrderService(), null);

        var result = await controller.GetAll();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenUserValid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeOrderService(), userId);

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenAdmin()
    {
        var controller = CreateController(new FakeOrderService(), ObjectId.GenerateNewId().ToString(), isAdmin: true);

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetById_ShouldReturnBadRequest_WhenOrderIdInvalid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeOrderService(), userId);

        var result = await controller.GetById("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenOrderMissing()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeOrderService(), userId);

        var result = await controller.GetById(ObjectId.GenerateNewId().ToString());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnForbid_WhenUserNotOwner()
    {
        var service = new FakeOrderService();
        var orderId = ObjectId.GenerateNewId().ToString();
        service.AddOrder(orderId, "owner123");

        var controller = CreateController(service, ObjectId.GenerateNewId().ToString());

        var result = await controller.GetById(orderId);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenUserIsOwner()
    {
        var service = new FakeOrderService();
        var userId = ObjectId.GenerateNewId().ToString();
        var orderId = ObjectId.GenerateNewId().ToString();
        service.AddOrder(orderId, userId);

        var controller = CreateController(service, userId);

        var result = await controller.GetById(orderId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenAdmin()
    {
        var service = new FakeOrderService();
        var orderId = ObjectId.GenerateNewId().ToString();
        service.AddOrder(orderId, "owner123");

        var controller = CreateController(service, ObjectId.GenerateNewId().ToString(), isAdmin: true);

        var result = await controller.GetById(orderId);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenUserIdMissing()
    {
        var controller = CreateController(new FakeOrderService(), null);

        var result = await controller.Create(new Order());

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenItemsMissing()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeOrderService(), userId);

        var result = await controller.Create(new Order { Items = new List<OrderItem>() });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenOrderNull()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeOrderService(), userId);

        var result = await controller.Create(null!);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenValid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeOrderService(), userId);

        var order = new Order
        {
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = "p1", Quantity = 1 }
            }
        };

        var result = await controller.Create(order);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenOrderIdInvalid()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeOrderService(), userId);

        var result = await controller.Update("invalid-id", new Order());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenOrderNull()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeOrderService(), userId);

        var result = await controller.Update(ObjectId.GenerateNewId().ToString(), null!);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenOrderMissing()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var controller = CreateController(new FakeOrderService(), userId);

        var result = await controller.Update(ObjectId.GenerateNewId().ToString(), new Order());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnForbid_WhenUserNotOwner()
    {
        var service = new FakeOrderService();
        var orderId = ObjectId.GenerateNewId().ToString();
        service.AddOrder(orderId, "owner123");

        var controller = CreateController(service, ObjectId.GenerateNewId().ToString());

        var result = await controller.Update(orderId, new Order());

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenValid()
    {
        var service = new FakeOrderService();
        var userId = ObjectId.GenerateNewId().ToString();
        var orderId = ObjectId.GenerateNewId().ToString();
        service.AddOrder(orderId, userId);

        var controller = CreateController(service, userId);

        var result = await controller.Update(orderId, new Order());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenUpdatingTwice()
    {
        var service = new FakeOrderService();
        var userId = ObjectId.GenerateNewId().ToString();
        var orderId = ObjectId.GenerateNewId().ToString();
        service.AddOrder(orderId, userId);

        var controller = CreateController(service, userId);

        var first = await controller.Update(orderId, new Order());
        var second = await controller.Update(orderId, new Order());

        first.Should().BeOfType<OkObjectResult>();
        second.Should().BeOfType<NotFoundObjectResult>();
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenOrderIdInvalid()
    {
        var controller = CreateController(new FakeOrderService(), ObjectId.GenerateNewId().ToString());

        var result = await controller.Delete("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenOrderMissing()
    {
        var controller = CreateController(new FakeOrderService(), ObjectId.GenerateNewId().ToString());

        var id = ObjectId.GenerateNewId().ToString();
        var result = await controller.Delete(id);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnForbid_WhenUserNotOwner()
    {
        var service = new FakeOrderService();
        var orderId = ObjectId.GenerateNewId().ToString();
        service.AddOrder(orderId, "owner123");

        var controller = CreateController(service, ObjectId.GenerateNewId().ToString());

        var result = await controller.Delete(orderId);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenValid()
    {
        var service = new FakeOrderService();
        var userId = ObjectId.GenerateNewId().ToString();
        var orderId = ObjectId.GenerateNewId().ToString();
        service.AddOrder(orderId, userId);

        var controller = CreateController(service, userId);

        var result = await controller.Delete(orderId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenDeletingTwice()
    {
        var service = new FakeOrderService();
        var userId = ObjectId.GenerateNewId().ToString();
        var orderId = ObjectId.GenerateNewId().ToString();
        service.AddOrder(orderId, userId);

        var controller = CreateController(service, userId);

        var first = await controller.Delete(orderId);
        var second = await controller.Delete(orderId);

        first.Should().BeOfType<OkObjectResult>();
        second.Should().BeOfType<NotFoundObjectResult>();
    }
}

// ---------------------------------------------------------
// FAKE SERVICE FOR TESTING
// ---------------------------------------------------------

public class FakeOrderService : IOrderService
{
    private readonly Dictionary<string, Order> _orders = new();

    public void AddOrder(string id, string userId)
    {
        _orders[id] = new Order
        {
            Id = id,
            UserId = userId,
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = "p1", Quantity = 1 }
            }
        };
    }

    public bool Exists(string id)
    {
        return _orders.ContainsKey(id);
    }

    public Task<List<Order>> GetAllAsync(bool isAdmin, string userId)
    {
        if (isAdmin)
            return Task.FromResult(_orders.Values.ToList());

        return Task.FromResult(_orders.Values.Where(o => o.UserId == userId).ToList());
    }

    public Task<Order?> GetByIdAsync(string id, bool isAdmin, string userId)
    {
        if (!_orders.ContainsKey(id))
            return Task.FromResult<Order?>(null);

        var order = _orders[id];

        if (isAdmin || order.UserId == userId)
            return Task.FromResult<Order?>(order);

        return Task.FromResult<Order?>(null);
    }

    public Task<Order?> CreateAsync(Order order)
    {
        if (order == null)
            return Task.FromResult<Order?>(null);

        order.Id = ObjectId.GenerateNewId().ToString();
        _orders[order.Id] = order;
        return Task.FromResult<Order?>(order);
    }

    public Task<bool> UpdateAsync(Order order, bool isAdmin, string userId)
    {
        if (order == null || string.IsNullOrWhiteSpace(order.Id))
            return Task.FromResult(false);

        if (!_orders.ContainsKey(order.Id))
            return Task.FromResult(false);

        var existing = _orders[order.Id];

        if (!isAdmin && existing.UserId != userId)
            return Task.FromResult(false);

        _orders[order.Id] = order;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(string id, bool isAdmin, string userId)
    {
        if (!_orders.ContainsKey(id))
            return Task.FromResult(false);

        var existing = _orders[id];

        if (!isAdmin && existing.UserId != userId)
            return Task.FromResult(false);

        _orders.Remove(id);
        return Task.FromResult(true);
    }
}

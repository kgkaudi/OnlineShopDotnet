using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Controllers;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;
using Xunit;
using MongoDB.Bson;
using System.Security.Claims;

public class CouponsControllerTests
{
    private CouponsController CreateController(ICouponService service, bool isAdmin = false)
    {
        var controller = new CouponsController(service);

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
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenAdmin()
    {
        var controller = CreateController(new FakeCouponService(), isAdmin: true);

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenCodeMissing()
    {
        var controller = CreateController(new FakeCouponService(), isAdmin: true);

        var result = await controller.Create(new Coupon
        {
            Code = "",
            Value = 10,
            Expiration = DateTime.UtcNow.AddDays(1)
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenValueInvalid()
    {
        var controller = CreateController(new FakeCouponService(), isAdmin: true);

        var result = await controller.Create(new Coupon
        {
            Code = "TEST",
            Value = 0,
            Expiration = DateTime.UtcNow.AddDays(1)
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenExpired()
    {
        var controller = CreateController(new FakeCouponService(), isAdmin: true);

        var result = await controller.Create(new Coupon
        {
            Code = "TEST",
            Value = 10,
            Expiration = DateTime.UtcNow.AddDays(-1)
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenValid()
    {
        var controller = CreateController(new FakeCouponService(), isAdmin: true);

        var result = await controller.Create(new Coupon
        {
            Code = "NEW10",
            Value = 10,
            Expiration = DateTime.UtcNow.AddDays(5)
        });

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenIdInvalid()
    {
        var controller = CreateController(new FakeCouponService(), isAdmin: true);

        var result = await controller.Delete("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenCouponMissing()
    {
        var controller = CreateController(new FakeCouponService(), isAdmin: true);

        var id = ObjectId.GenerateNewId().ToString();
        var result = await controller.Delete(id);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenValid()
    {
        var service = new FakeCouponService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddCoupon(id, "TEST", 10);

        var controller = CreateController(service, isAdmin: true);

        var result = await controller.Delete(id);

        result.Should().BeOfType<NoContentResult>();
    }

    // ---------------------------------------------------------
    // VALIDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldReturnBadRequest_WhenCodeMissing()
    {
        var controller = CreateController(new FakeCouponService());

        var result = await controller.Validate("");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Validate_ShouldReturnBadRequest_WhenInvalid()
    {
        var controller = CreateController(new FakeCouponService());

        var result = await controller.Validate("INVALID");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Validate_ShouldReturnOk_WhenValid()
    {
        var service = new FakeCouponService();
        service.AddCoupon(ObjectId.GenerateNewId().ToString(), "SAVE10", 10);

        var controller = CreateController(service);

        var result = await controller.Validate("SAVE10");

        result.Should().BeOfType<OkObjectResult>();
    }
}

// ---------------------------------------------------------
// FAKE SERVICE FOR TESTING
// ---------------------------------------------------------

public class FakeCouponService : ICouponService
{
    private readonly Dictionary<string, Coupon> _store = new();

    public void AddCoupon(string id, string code, decimal value)
    {
        _store[id] = new Coupon
        {
            Id = id,
            Code = code,
            Value = value,
            Expiration = DateTime.UtcNow.AddDays(5),
            Active = true
        };
    }

    public Task<List<Coupon>> GetAllAsync()
        => Task.FromResult(_store.Values.ToList());

    public Task<Coupon?> CreateAsync(Coupon coupon)
    {
        var id = ObjectId.GenerateNewId().ToString();
        coupon.Id = id;
        _store[id] = coupon;
        return Task.FromResult<Coupon?>(coupon);
    }

    public Task<bool> DeleteAsync(string id)
        => Task.FromResult(_store.Remove(id));

    public Task<Coupon?> ValidateAsync(string code)
        => Task.FromResult(_store.Values.FirstOrDefault(c => c.Code == code));
}

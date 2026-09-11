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
    public async Task GetAll_ShouldReturnUnauthorized_WhenNotAdmin()
    {
        var controller = CreateController(new FakeCouponService(), isAdmin: false);

        var result = await controller.GetAll();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

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
    public async Task Create_ShouldReturnUnauthorized_WhenNotAdmin()
    {
        var controller = CreateController(new FakeCouponService(), isAdmin: false);

        var result = await controller.Create(new Coupon
        {
            Code = "X",
            Value = 10,
            Expiration = DateTime.UtcNow.AddDays(1)
        });

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenCouponNull()
    {
        var controller = CreateController(new FakeCouponService(), isAdmin: true);

        var result = await controller.Create(null!);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

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

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDuplicateCode()
    {
        var service = new FakeCouponService();
        service.AddCoupon(ObjectId.GenerateNewId().ToString(), "DUP", 10);

        var controller = CreateController(service, isAdmin: true);

        var result = await controller.Create(new Coupon
        {
            Code = "DUP",
            Value = 5,
            Expiration = DateTime.UtcNow.AddDays(2)
        });

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task Delete_ShouldReturnUnauthorized_WhenNotAdmin()
    {
        var controller = CreateController(new FakeCouponService(), isAdmin: false);

        var result = await controller.Delete(ObjectId.GenerateNewId().ToString());

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

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

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenDeletingTwice()
    {
        var service = new FakeCouponService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddCoupon(id, "TEST", 10);

        var controller = CreateController(service, isAdmin: true);

        var first = await controller.Delete(id);
        var second = await controller.Delete(id);

        first.Should().BeOfType<NoContentResult>();
        second.Should().BeOfType<NotFoundObjectResult>();
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
    public async Task Validate_ShouldReturnBadRequest_WhenCodeNull()
    {
        var controller = CreateController(new FakeCouponService());

        var result = await controller.Validate(null!);

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
    public async Task Validate_ShouldReturnBadRequest_WhenInactive()
    {
        var service = new FakeCouponService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddCoupon(id, "SAVE10", 10);
        service.SetInactive(id);

        var controller = CreateController(service);

        var result = await controller.Validate("SAVE10");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Validate_ShouldReturnBadRequest_WhenExpired()
    {
        var service = new FakeCouponService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddCoupon(id, "OLD", 10);
        service.SetExpired(id);

        var controller = CreateController(service);

        var result = await controller.Validate("OLD");

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

    [Fact]
    public async Task Validate_ShouldReturnOk_WhenCaseInsensitive()
    {
        var service = new FakeCouponService();
        service.AddCoupon(ObjectId.GenerateNewId().ToString(), "SAVE10", 10);

        var controller = CreateController(service);

        var result = await controller.Validate("save10");

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

    public void SetInactive(string id)
    {
        if (_store.ContainsKey(id))
            _store[id].Active = false;
    }

    public void SetExpired(string id)
    {
        if (_store.ContainsKey(id))
            _store[id].Expiration = DateTime.UtcNow.AddDays(-1);
    }

    public Task<List<Coupon>> GetAllAsync()
        => Task.FromResult(_store.Values.ToList());

    public Task<Coupon?> CreateAsync(Coupon coupon)
    {
        if (coupon == null)
            return Task.FromResult<Coupon?>(null);

        var id = ObjectId.GenerateNewId().ToString();
        coupon.Id = id;
        _store[id] = coupon;
        return Task.FromResult<Coupon?>(coupon);
    }

    public Task<bool> DeleteAsync(string id)
        => Task.FromResult(_store.Remove(id));

    public Task<Coupon?> ValidateAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Task.FromResult<Coupon?>(null);

        var match = _store.Values.FirstOrDefault(c =>
            c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult<Coupon?>(match);
    }
}

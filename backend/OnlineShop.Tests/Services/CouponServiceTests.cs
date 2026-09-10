using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using OnlineShop.Api.Services;
using Xunit;

public class CouponServiceTests : RepositoryTestBase
{
    private readonly CouponService _service;
    private readonly CouponRepository _repo;
    private readonly IMongoCollection<Coupon> _coupons;

    public CouponServiceTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new CouponRepository(config);
        _service = new CouponService(_repo);

        _coupons = Fixture.Database.GetCollection<Coupon>("Coupons");
        Fixture.Database.DropCollection("Coupons");
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoCoupons()
    {
        var result = await _service.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCoupons()
    {
        await _repo.CreateAsync(new Coupon
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Code = "A",
            Value = 10,
            Type = "percentage",
            Expiration = DateTime.UtcNow.AddDays(1),
            MaxUsage = 10
        });

        await _repo.CreateAsync(new Coupon
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Code = "B",
            Value = 5,
            Type = "fixed",
            Expiration = DateTime.UtcNow.AddDays(1),
            MaxUsage = 10
        });

        var result = await _service.GetAllAsync();
        result.Should().HaveCount(2);
    }

    // ---------------------------------------------------------
    // GET BY CODE
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnNull_WhenCodeEmpty()
    {
        var result = await _service.GetByCodeAsync("");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetByCodeAsync("MISSING");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnCoupon_WhenExists()
    {
        var coupon = new Coupon
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Code = "SAVE10",
            Value = 10,
            Type = "percentage",
            Expiration = DateTime.UtcNow.AddDays(1),
            MaxUsage = 10
        };

        await _repo.CreateAsync(coupon);

        var result = await _service.GetByCodeAsync("SAVE10");
        result.Should().NotBeNull();
        result!.Code.Should().Be("SAVE10");
    }

    // ---------------------------------------------------------
    // VALIDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task ValidateAsync_ShouldReturnNull_WhenCodeEmpty()
    {
        var result = await _service.ValidateAsync("");
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnNull_WhenCouponNotFound()
    {
        var result = await _service.ValidateAsync("MISSING");
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnNull_WhenInactive()
    {
        var coupon = new Coupon
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Code = "INACTIVE",
            Active = false,
            Expiration = DateTime.UtcNow.AddDays(1),
            MaxUsage = 10
        };

        await _repo.CreateAsync(coupon);

        var result = await _service.ValidateAsync("INACTIVE");
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnNull_WhenExpired()
    {
        var coupon = new Coupon
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Code = "OLD",
            Active = true,
            Expiration = DateTime.UtcNow.AddDays(-1),
            MaxUsage = 10
        };

        await _repo.CreateAsync(coupon);

        var result = await _service.ValidateAsync("OLD");
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnNull_WhenMaxUsageReached()
    {
        var coupon = new Coupon
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Code = "LIMIT",
            Active = true,
            Expiration = DateTime.UtcNow.AddDays(1),
            MaxUsage = 1,
            UsedCount = 1
        };

        await _repo.CreateAsync(coupon);

        var result = await _service.ValidateAsync("LIMIT");
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnCoupon_WhenValid()
    {
        var coupon = new Coupon
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Code = "GOOD",
            Active = true,
            Expiration = DateTime.UtcNow.AddDays(1),
            MaxUsage = 10,
            UsedCount = 0
        };

        await _repo.CreateAsync(coupon);

        var result = await _service.ValidateAsync("GOOD");
        result.Should().NotBeNull();
        result!.Code.Should().Be("GOOD");
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenCodeEmpty()
    {
        var result = await _service.CreateAsync(new Coupon { Code = "" });
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCoupon()
    {
        var coupon = new Coupon
        {
            Code = "NEW",
            Value = 10,
            Type = "percentage",
            Expiration = DateTime.UtcNow.AddDays(1),
            MaxUsage = 10
        };

        var created = await _service.CreateAsync(coupon);

        created.Should().NotBeNull();
        created!.Id.Should().NotBeNull();

        var fetched = await _repo.GetByCodeAsync("NEW");
        fetched.Should().NotBeNull();
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdInvalid()
    {
        var result = await _service.DeleteAsync("invalid-id");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.DeleteAsync(id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCoupon_WhenExists()
    {
        var coupon = new Coupon
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Code = "DELETE",
            Value = 5,
            Type = "fixed",
            Expiration = DateTime.UtcNow.AddDays(1),
            MaxUsage = 10
        };

        await _repo.CreateAsync(coupon);

        var deleted = await _service.DeleteAsync(coupon.Id);
        deleted.Should().BeTrue();

        var fetched = await _repo.GetByCodeAsync("DELETE");
        fetched.Should().BeNull();
    }
}

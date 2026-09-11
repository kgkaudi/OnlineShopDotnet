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
    // GET BY CODE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnNull_WhenCodeIsNull()
    {
        var result = await _service.GetByCodeAsync(null!);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnNull_WhenCodeIsWhitespace()
    {
        var result = await _service.GetByCodeAsync("   ");
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
    // VALIDATE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task ValidateAsync_ShouldReturnNull_WhenCodeIsNull()
    {
        var result = await _service.ValidateAsync(null!);
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnNull_WhenCodeIsWhitespace()
    {
        var result = await _service.ValidateAsync("   ");
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
            Type = "percentage",
            Value = 10,
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
    // CREATE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenCouponIsNull()
    {
        var result = await _service.CreateAsync(null!);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenCodeIsNull()
    {
        var result = await _service.CreateAsync(new Coupon { Code = null! });
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenCodeIsWhitespace()
    {
        var result = await _service.CreateAsync(new Coupon { Code = "   " });
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = await _service.CreateAsync(new Coupon { Code = "X", Type = null! });
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenTypeIsWhitespace()
    {
        var result = await _service.CreateAsync(new Coupon { Code = "X", Type = "   " });
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenValueIsZeroOrNegative()
    {
        var result1 = await _service.CreateAsync(new Coupon { Code = "X", Type = "percentage", Value = 0 });
        var result2 = await _service.CreateAsync(new Coupon { Code = "Y", Type = "percentage", Value = -5 });

        result1.Should().BeNull();
        result2.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenExpirationMissing()
    {
        var result = await _service.CreateAsync(new Coupon { Code = "X", Type = "percentage", Value = 10 });
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenMaxUsageZeroOrNegative()
    {
        var result1 = await _service.CreateAsync(new Coupon { Code = "X", Type = "percentage", Value = 10, Expiration = DateTime.UtcNow.AddDays(1), MaxUsage = 0 });
        var result2 = await _service.CreateAsync(new Coupon { Code = "Y", Type = "percentage", Value = 10, Expiration = DateTime.UtcNow.AddDays(1), MaxUsage = -3 });

        result1.Should().BeNull();
        result2.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenCodeAlreadyExists()
    {
        await _repo.CreateAsync(new Coupon
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Code = "DUP",
            Type = "percentage",
            Value = 10,
            Expiration = DateTime.UtcNow.AddDays(1),
            MaxUsage = 10
        });

        var result = await _service.CreateAsync(new Coupon
        {
            Code = "DUP",
            Type = "percentage",
            Value = 10,
            Expiration = DateTime.UtcNow.AddDays(1),
            MaxUsage = 10
        });

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimCode()
    {
        var created = await _service.CreateAsync(new Coupon
        {
            Code = "   NEW   ",
            Type = "percentage",
            Value = 10,
            Expiration = DateTime.UtcNow.AddDays(1),
            MaxUsage = 10
        });

        created.Should().NotBeNull();
        created!.Code.Should().Be("NEW");
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
    // DELETE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsNull()
    {
        var result = await _service.DeleteAsync(null!);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsWhitespace()
    {
        var result = await _service.DeleteAsync("   ");
        result.Should().BeFalse();
    }

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

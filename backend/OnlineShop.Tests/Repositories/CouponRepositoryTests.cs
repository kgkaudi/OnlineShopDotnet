using FluentAssertions;
using MongoDB.Bson;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using Xunit;

public class CouponRepositoryTests : RepositoryTestBase
{
    private readonly CouponRepository _repo;

    public CouponRepositoryTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new CouponRepository(config);

        Fixture.Database.DropCollection("Coupons");
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoCoupons()
    {
        var result = await _repo.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCoupons()
    {
        await _repo.CreateAsync(new Coupon { Code = "A", Type = "percentage", Value = 10 });
        await _repo.CreateAsync(new Coupon { Code = "B", Type = "fixed", Value = 5 });

        var result = await _repo.GetAllAsync();
        result.Should().HaveCount(2);
    }

    // ---------------------------------------------------------
    // GET BY CODE
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repo.GetByCodeAsync("missing");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnCoupon_WhenExists()
    {
        var coupon = new Coupon { Code = "SUMMER", Type = "percentage", Value = 20 };
        await _repo.CreateAsync(coupon);

        var result = await _repo.GetByCodeAsync("SUMMER");
        result.Should().NotBeNull();
        result!.Code.Should().Be("SUMMER");
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsInvalid()
    {
        var result = await _repo.GetByIdAsync("invalid-id");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCouponDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _repo.GetByIdAsync(id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCoupon_WhenExists()
    {
        var coupon = new Coupon { Code = "WINTER", Type = "fixed", Value = 15 };
        await _repo.CreateAsync(coupon);

        var result = await _repo.GetByIdAsync(coupon.Id);
        result.Should().NotBeNull();
        result!.Code.Should().Be("WINTER");
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldInsertCoupon()
    {
        var coupon = new Coupon { Code = "NEW", Type = "percentage", Value = 30 };
        await _repo.CreateAsync(coupon);

        var fetched = await _repo.GetByIdAsync(coupon.Id);
        fetched.Should().NotBeNull();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsInvalid()
    {
        var coupon = new Coupon
        {
            Id = "invalid-id",
            Code = "BAD",
            Type = "percentage",
            Value = 10
        };

        var result = await _repo.UpdateAsync(coupon);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenCouponDoesNotExist()
    {
        var coupon = new Coupon
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Code = "MISSING",
            Type = "fixed",
            Value = 5
        };

        var result = await _repo.UpdateAsync(coupon);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCoupon_WhenExists()
    {
        var coupon = new Coupon { Code = "EDIT", Type = "percentage", Value = 10 };
        await _repo.CreateAsync(coupon);

        coupon.Value = 25;

        var updated = await _repo.UpdateAsync(coupon);
        updated.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(coupon.Id);
        fetched!.Value.Should().Be(25);
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsInvalid()
    {
        var result = await _repo.DeleteAsync("invalid-id");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenCouponDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _repo.DeleteAsync(id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCoupon_WhenExists()
    {
        var coupon = new Coupon { Code = "REMOVE", Type = "fixed", Value = 50 };
        await _repo.CreateAsync(coupon);

        var deleted = await _repo.DeleteAsync(coupon.Id);
        deleted.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(coupon.Id);
        fetched.Should().BeNull();
    }
}

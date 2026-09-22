using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using Xunit;

public class OrderRepositoryTests : RepositoryTestBase
{
    private readonly OrderRepository _repo;
    private readonly IMongoCollection<Order> _orders;

    public OrderRepositoryTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new OrderRepository(config);
        _orders = Fixture.Database.GetCollection<Order>("Order");

        Fixture.Database.DropCollection("Order");
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoOrders()
    {
        var result = await _repo.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllOrders()
    {
        await _repo.CreateAsync(new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 10
        });

        await _repo.CreateAsync(new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 20
        });

        var result = await _repo.GetAllAsync();
        result.Should().HaveCount(2);
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsNull()
    {
        var result = await _repo.GetByIdAsync(null!);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsWhitespace()
    {
        var result = await _repo.GetByIdAsync("   ");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsInvalid()
    {
        var result = await _repo.GetByIdAsync("invalid-id");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _repo.GetByIdAsync(id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenExists()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 50
        };

        await _repo.CreateAsync(order);

        var result = await _repo.GetByIdAsync(order.Id);
        result.Should().NotBeNull();
        result!.Total.Should().Be(50);
    }

    // ---------------------------------------------------------
    // GET BY USER ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserIdIsNull()
    {
        var result = await _repo.GetByUserIdAsync(null!);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserIdIsWhitespace()
    {
        var result = await _repo.GetByUserIdAsync("   ");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserIdIsInvalid()
    {
        var result = await _repo.GetByUserIdAsync("invalid-id");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserHasNoOrders()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var result = await _repo.GetByUserIdAsync(userId);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOrders_WhenUserHasOrders()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        await _repo.CreateAsync(new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            Total = 10
        });

        await _repo.CreateAsync(new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            Total = 20
        });

        var result = await _repo.GetByUserIdAsync(userId);
        result.Should().HaveCount(2);
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenOrderIsNull()
    {
        Func<Task> act = async () => await _repo.CreateAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenIdIsMissing()
    {
        var order = new Order
        {
            Id = null!,
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 10
        };

        Func<Task> act = async () => await _repo.CreateAsync(order);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUserIdIsMissing()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = null!,
            Total = 10
        };

        Func<Task> act = async () => await _repo.CreateAsync(order);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenTotalIsZeroOrNegative()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 0
        };

        Func<Task> act = async () => await _repo.CreateAsync(order);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldInsertOrder()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 99
        };

        await _repo.CreateAsync(order);

        var fetched = await _repo.GetByIdAsync(order.Id);
        fetched.Should().NotBeNull();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenOrderIsNull()
    {
        var result = await _repo.UpdateAsync(null!);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsMissing()
    {
        var order = new Order
        {
            Id = null!,
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 10
        };

        var result = await _repo.UpdateAsync(order);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsWhitespace()
    {
        var order = new Order
        {
            Id = "   ",
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 10
        };

        var result = await _repo.UpdateAsync(order);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsInvalid()
    {
        var order = new Order
        {
            Id = "invalid-id",
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 10
        };

        var result = await _repo.UpdateAsync(order);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenUserIdIsMissing()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = null!,
            Total = 10
        };

        var result = await _repo.UpdateAsync(order);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenTotalIsZeroOrNegative()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 10
        };

        await _repo.CreateAsync(order);

        order.Total = 0;

        var result = await _repo.UpdateAsync(order);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenOrderDoesNotExist()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 10
        };

        var result = await _repo.UpdateAsync(order);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateOrder_WhenExists()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 10
        };

        await _repo.CreateAsync(order);

        order.Total = 200;

        var updated = await _repo.UpdateAsync(order);
        updated.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(order.Id);
        fetched!.Total.Should().Be(200);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotAffectOtherOrders()
    {
        var o1 = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 10
        };

        var o2 = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 50
        };

        await _repo.CreateAsync(o1);
        await _repo.CreateAsync(o2);

        o1.Total = 999;
        await _repo.UpdateAsync(o1);

        var fetched2 = await _repo.GetByIdAsync(o2.Id);
        fetched2!.Total.Should().Be(50);
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsNull()
    {
        var result = await _repo.DeleteAsync(null!);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsWhitespace()
    {
        var result = await _repo.DeleteAsync("   ");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsInvalid()
    {
        var result = await _repo.DeleteAsync("invalid-id");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenOrderDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _repo.DeleteAsync(id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteOrder_WhenExists()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 123
        };

        await _repo.CreateAsync(order);

        var deleted = await _repo.DeleteAsync(order.Id);
        deleted.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(order.Id);
        fetched.Should().BeNull();
    }

    // ---------------------------------------------------------
    // CANCEL
    // ---------------------------------------------------------

    [Fact]
    public async Task CancelAsync_ShouldReturnFalse_WhenIdIsNull()
    {
        var result = await _repo.CancelAsync(null!, ObjectId.GenerateNewId().ToString(), false);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnFalse_WhenIdIsInvalid()
    {
        var result = await _repo.CancelAsync("invalid-id", ObjectId.GenerateNewId().ToString(), false);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnFalse_WhenUserIdIsNull()
    {
        var result = await _repo.CancelAsync(ObjectId.GenerateNewId().ToString(), null!, false);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnFalse_WhenUserIdIsInvalid()
    {
        var result = await _repo.CancelAsync(ObjectId.GenerateNewId().ToString(), "invalid-id", false);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnFalse_WhenUserIdIsInvalid_EvenForAdmin()
    {
        // The userId shape is still validated up front, even though an
        // admin's userId isn't used as an ownership filter.
        var result = await _repo.CancelAsync(ObjectId.GenerateNewId().ToString(), "invalid-id", true);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnFalse_WhenOrderDoesNotExist()
    {
        var result = await _repo.CancelAsync(
            ObjectId.GenerateNewId().ToString(),
            ObjectId.GenerateNewId().ToString(),
            false);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnFalse_WhenNotOwnerAndNotAdmin()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 10,
            Status = "Pending"
        };
        await _repo.CreateAsync(order);

        var result = await _repo.CancelAsync(
            order.Id,
            ObjectId.GenerateNewId().ToString(), // different user
            false);

        result.Should().BeFalse();

        var fetched = await _repo.GetByIdAsync(order.Id);
        fetched!.Status.Should().Be("Pending"); // unchanged
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnTrue_WhenOwnerCancelsPendingOrder()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            Total = 10,
            Status = "Pending"
        };
        await _repo.CreateAsync(order);

        var result = await _repo.CancelAsync(order.Id, userId, false);
        result.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(order.Id);
        fetched!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnTrue_WhenOwnerCancelsProcessingOrder()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            Total = 10,
            Status = "Processing"
        };
        await _repo.CreateAsync(order);

        var result = await _repo.CancelAsync(order.Id, userId, false);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("Shipped")]
    [InlineData("Delivered")]
    [InlineData("Cancelled")]
    public async Task CancelAsync_ShouldReturnFalse_WhenStatusNotCancelable(string status)
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            Total = 10,
            Status = status
        };
        await _repo.CreateAsync(order);

        var result = await _repo.CancelAsync(order.Id, userId, false);
        result.Should().BeFalse();

        var fetched = await _repo.GetByIdAsync(order.Id);
        fetched!.Status.Should().Be(status); // unchanged
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnFalse_WhenStatusNotCancelable_EvenForAdmin()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 10,
            Status = "Shipped"
        };
        await _repo.CreateAsync(order);

        var result = await _repo.CancelAsync(order.Id, ObjectId.GenerateNewId().ToString(), true);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnTrue_WhenAdminCancelsAnotherUsersOrder()
    {
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Total = 10,
            Status = "Pending"
        };
        await _repo.CreateAsync(order);

        var result = await _repo.CancelAsync(order.Id, ObjectId.GenerateNewId().ToString(), true);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("PENDING")]
    [InlineData("pending")]
    [InlineData("PROCESSING")]
    [InlineData("processing")]
    public async Task CancelAsync_ShouldBeCaseInsensitive_ForStatusMatching(string storedStatus)
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            Total = 10,
            Status = storedStatus
        };
        await _repo.CreateAsync(order);

        var result = await _repo.CancelAsync(order.Id, userId, false);
        result.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(order.Id);
        fetched!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnFalse_WhenCancellingAlreadyCancelledOrder()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var order = new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            Total = 10,
            Status = "Pending"
        };
        await _repo.CreateAsync(order);

        var first = await _repo.CancelAsync(order.Id, userId, false);
        var second = await _repo.CancelAsync(order.Id, userId, false);

        first.Should().BeTrue();
        second.Should().BeFalse();
    }
}

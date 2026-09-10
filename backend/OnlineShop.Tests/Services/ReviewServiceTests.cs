using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using OnlineShop.Api.Services;
using Xunit;

public class ReviewServiceTests : RepositoryTestBase
{
    private readonly ReviewService _service;
    private readonly ReviewRepository _repo;
    private readonly IMongoCollection<Review> _reviews;

    public ReviewServiceTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new ReviewRepository(config);
        _service = new ReviewService(_repo);

        _reviews = Fixture.Database.GetCollection<Review>("Reviews");
        Fixture.Database.DropCollection("Reviews");
    }

    // ---------------------------------------------------------
    // GET BY PRODUCT
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByProductIdAsync_ShouldReturnEmpty_WhenProductIdInvalid()
    {
        var result = await _service.GetByProductIdAsync("invalid-id");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByProductIdAsync_ShouldReturnReviews_WhenProductMatches()
    {
        var productId = ObjectId.GenerateNewId().ToString();
        var otherProductId = ObjectId.GenerateNewId().ToString();

        await _repo.CreateAsync(new Review
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ProductId = productId,
            UserId = ObjectId.GenerateNewId().ToString(),
            Rating = 5,
            Comment = "Great!"
        });

        await _repo.CreateAsync(new Review
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ProductId = otherProductId,
            UserId = ObjectId.GenerateNewId().ToString(),
            Rating = 3,
            Comment = "Okay"
        });

        var result = await _service.GetByProductIdAsync(productId);

        result.Should().HaveCount(1);
        result[0].Comment.Should().Be("Great!");
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldGenerateId_WhenMissing()
    {
        var review = new Review
        {
            ProductId = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Rating = 4,
            Comment = "Nice"
        };

        var created = await _service.CreateAsync(review);

        created.Id.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenProductIdInvalid()
    {
        var review = new Review
        {
            ProductId = "invalid-id",
            UserId = ObjectId.GenerateNewId().ToString(),
            Rating = 4,
            Comment = "Nice"
        };

        Func<Task> act = async () => await _service.CreateAsync(review);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUserIdInvalid()
    {
        var review = new Review
        {
            ProductId = ObjectId.GenerateNewId().ToString(),
            UserId = "invalid-id",
            Rating = 4,
            Comment = "Nice"
        };

        Func<Task> act = async () => await _service.CreateAsync(review);
        await act.Should().ThrowAsync<ArgumentException>();
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
    public async Task DeleteAsync_ShouldReturnFalse_WhenReviewNotFound()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.DeleteAsync(id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDelete_WhenExists()
    {
        var review = new Review
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ProductId = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Rating = 5,
            Comment = "Delete me"
        };

        await _repo.CreateAsync(review);

        var deleted = await _service.DeleteAsync(review.Id);
        deleted.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(review.Id);
        fetched.Should().BeNull();
    }
}

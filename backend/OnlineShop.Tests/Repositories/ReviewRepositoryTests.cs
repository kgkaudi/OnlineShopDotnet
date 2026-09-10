using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using Xunit;

public class ReviewRepositoryTests : RepositoryTestBase
{
    private readonly ReviewRepository _repo;
    private readonly IMongoCollection<Review> _reviews;

    public ReviewRepositoryTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new ReviewRepository(config);
        _reviews = Fixture.Database.GetCollection<Review>("Reviews");

        Fixture.Database.DropCollection("Reviews");
    }

    // ---------------------------------------------------------
    // GET BY PRODUCT ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByProductIdAsync_ShouldReturnEmptyList_WhenIdIsInvalid()
    {
        var result = await _repo.GetByProductIdAsync("invalid-id");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByProductIdAsync_ShouldReturnEmptyList_WhenNoReviews()
    {
        var productId = ObjectId.GenerateNewId().ToString();
        var result = await _repo.GetByProductIdAsync(productId);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByProductIdAsync_ShouldReturnReviews_WhenExists()
    {
        var productId = ObjectId.GenerateNewId().ToString();

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
            ProductId = productId,
            UserId = ObjectId.GenerateNewId().ToString(),
            Rating = 4,
            Comment = "Good!"
        });

        var result = await _repo.GetByProductIdAsync(productId);
        result.Should().HaveCount(2);
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
    public async Task GetByIdAsync_ShouldReturnNull_WhenReviewDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _repo.GetByIdAsync(id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnReview_WhenExists()
    {
        var review = new Review
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ProductId = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Rating = 5,
            Comment = "Perfect!"
        };

        await _repo.CreateAsync(review);

        var result = await _repo.GetByIdAsync(review.Id);
        result.Should().NotBeNull();
        result!.Comment.Should().Be("Perfect!");
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldInsertReview()
    {
        var review = new Review
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ProductId = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Rating = 3,
            Comment = "Ok"
        };

        await _repo.CreateAsync(review);

        var fetched = await _repo.GetByIdAsync(review.Id);
        fetched.Should().NotBeNull();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsInvalid()
    {
        var review = new Review
        {
            Id = "invalid-id",
            ProductId = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Rating = 2,
            Comment = "Bad"
        };

        var result = await _repo.UpdateAsync(review);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenReviewDoesNotExist()
    {
        var review = new Review
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ProductId = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Rating = 2,
            Comment = "Missing"
        };

        var result = await _repo.UpdateAsync(review);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateReview_WhenExists()
    {
        var review = new Review
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ProductId = ObjectId.GenerateNewId().ToString(),
            UserId = ObjectId.GenerateNewId().ToString(),
            Rating = 3,
            Comment = "Average"
        };

        await _repo.CreateAsync(review);

        review.Comment = "Updated";

        var updated = await _repo.UpdateAsync(review);
        updated.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(review.Id);
        fetched!.Comment.Should().Be("Updated");
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
    public async Task DeleteAsync_ShouldReturnFalse_WhenReviewDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _repo.DeleteAsync(id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteReview_WhenExists()
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

        var deleted = await _repo.DeleteAsync(review.Id);
        deleted.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(review.Id);
        fetched.Should().BeNull();
    }
}

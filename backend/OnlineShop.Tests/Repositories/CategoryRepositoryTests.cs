using FluentAssertions;
using MongoDB.Bson;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using Xunit;

public class CategoryRepositoryTests : RepositoryTestBase
{
    private readonly CategoryRepository _repo;

    public CategoryRepositoryTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new CategoryRepository(config);

        // Ensure Categories collection is clean
        Fixture.Database.DropCollection("Categories");
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoCategories()
    {
        var result = await _repo.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        await _repo.CreateAsync(new Category { Name = "A" });
        await _repo.CreateAsync(new Category { Name = "B" });

        var result = await _repo.GetAllAsync();
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
    public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _repo.GetByIdAsync(id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategory_WhenExists()
    {
        var category = new Category { Name = "Electronics" };
        await _repo.CreateAsync(category);

        var result = await _repo.GetByIdAsync(category.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Electronics");
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldInsertCategory()
    {
        var category = new Category { Name = "Books" };
        await _repo.CreateAsync(category);

        var fetched = await _repo.GetByIdAsync(category.Id);
        fetched.Should().NotBeNull();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsInvalid()
    {
        var category = new Category
        {
            Id = "invalid-id",
            Name = "Invalid"
        };

        var result = await _repo.UpdateAsync(category);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
    {
        var category = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Missing"
        };

        var result = await _repo.UpdateAsync(category);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategory_WhenExists()
    {
        var category = new Category { Name = "Old Name" };
        await _repo.CreateAsync(category);

        category.Name = "New Name";

        var updated = await _repo.UpdateAsync(category);
        updated.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(category.Id);
        fetched!.Name.Should().Be("New Name");
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
    public async Task DeleteAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _repo.DeleteAsync(id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCategory_WhenExists()
    {
        var category = new Category { Name = "Delete Me" };
        await _repo.CreateAsync(category);

        var deleted = await _repo.DeleteAsync(category.Id);
        deleted.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(category.Id);
        fetched.Should().BeNull();
    }
}

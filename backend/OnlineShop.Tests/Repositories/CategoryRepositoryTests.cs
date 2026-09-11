using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
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
    public async Task CreateAsync_ShouldThrow_WhenCategoryIsNull()
    {
        Func<Task> act = async () => await _repo.CreateAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNameIsMissing()
    {
        var category = new Category { Name = null! };

        Func<Task> act = async () => await _repo.CreateAsync(category);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

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
    public async Task UpdateAsync_ShouldReturnFalse_WhenCategoryIsNull()
    {
        var result = await _repo.UpdateAsync(null!);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsNull()
    {
        var category = new Category { Id = null!, Name = "Test" };
        var result = await _repo.UpdateAsync(category);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsWhitespace()
    {
        var category = new Category { Id = "   ", Name = "Test" };
        var result = await _repo.UpdateAsync(category);
        result.Should().BeFalse();
    }

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
    public async Task UpdateAsync_ShouldReturnFalse_WhenNameIsMissing()
    {
        var category = new Category { Name = "Old Name" };
        await _repo.CreateAsync(category);

        category.Name = null!;

        var result = await _repo.UpdateAsync(category);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenReplaceMatchedButNotModified()
    {
        var category = new Category { Name = "Same Name" };
        await _repo.CreateAsync(category);

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

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
    }

    // ---------------------------------------------------------
    // TEST SETUP
    // ---------------------------------------------------------

    private void ClearCategories()
    {
        Fixture.Database.DropCollection("Category");
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoCategories()
    {
        ClearCategories();

        var result = await _repo.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        ClearCategories();

        await _repo.CreateAsync(new Category { Name = "A" });
        await _repo.CreateAsync(new Category { Name = "B" });

        var result = await _repo.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().ContainSingle(c => c.Name == "A");
        result.Should().ContainSingle(c => c.Name == "B");
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsNull()
    {
        ClearCategories();

        var result = await _repo.GetByIdAsync(null!);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsWhitespace()
    {
        ClearCategories();

        var result = await _repo.GetByIdAsync("   ");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsInvalid()
    {
        ClearCategories();

        var result = await _repo.GetByIdAsync("invalid-id");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        ClearCategories();

        var id = ObjectId.GenerateNewId().ToString();

        var result = await _repo.GetByIdAsync(id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategory_WhenExists()
    {
        ClearCategories();

        var category = new Category
        {
            Name = "Electronics"
        };

        await _repo.CreateAsync(category);

        var result = await _repo.GetByIdAsync(category.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
        result.Name.Should().Be("Electronics");
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCategoryIsNull()
    {
        ClearCategories();

        Func<Task> act = async () =>
            await _repo.CreateAsync(null!);

        await act.Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNameIsMissing()
    {
        ClearCategories();

        var category = new Category
        {
            Name = null!
        };

        Func<Task> act = async () =>
            await _repo.CreateAsync(category);

        await act.Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldInsertCategory()
    {
        ClearCategories();

        var category = new Category
        {
            Name = "Books"
        };

        await _repo.CreateAsync(category);

        var fetched = await _repo.GetByIdAsync(category.Id);

        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("Books");
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenCategoryIsNull()
    {
        ClearCategories();

        var result = await _repo.UpdateAsync(null!);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsNull()
    {
        ClearCategories();

        var category = new Category
        {
            Id = null!,
            Name = "Test"
        };

        var result = await _repo.UpdateAsync(category);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsWhitespace()
    {
        ClearCategories();

        var category = new Category
        {
            Id = "   ",
            Name = "Test"
        };

        var result = await _repo.UpdateAsync(category);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsInvalid()
    {
        ClearCategories();

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
        ClearCategories();

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
        ClearCategories();

        var category = new Category
        {
            Name = "Old Name"
        };

        await _repo.CreateAsync(category);

        category.Name = null!;

        var result = await _repo.UpdateAsync(category);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenReplaceMatchedButNotModified()
    {
        ClearCategories();

        var category = new Category
        {
            Name = "Same Name"
        };

        await _repo.CreateAsync(category);

        var result = await _repo.UpdateAsync(category);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategory_WhenExists()
    {
        ClearCategories();

        var category = new Category
        {
            Name = "Old Name"
        };

        await _repo.CreateAsync(category);

        category.Name = "New Name";

        var updated = await _repo.UpdateAsync(category);

        updated.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(category.Id);

        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("New Name");
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsNull()
    {
        ClearCategories();

        var result = await _repo.DeleteAsync(null!);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsWhitespace()
    {
        ClearCategories();

        var result = await _repo.DeleteAsync("   ");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsInvalid()
    {
        ClearCategories();

        var result = await _repo.DeleteAsync("invalid-id");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
    {
        ClearCategories();

        var id = ObjectId.GenerateNewId().ToString();

        var result = await _repo.DeleteAsync(id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCategory_WhenExists()
    {
        ClearCategories();

        var category = new Category
        {
            Name = "Delete Me"
        };

        await _repo.CreateAsync(category);

        var deleted = await _repo.DeleteAsync(category.Id);

        deleted.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(category.Id);

        fetched.Should().BeNull();
    }
}
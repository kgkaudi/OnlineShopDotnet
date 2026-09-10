using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using OnlineShop.Api.Services;
using Xunit;

public class CategoryServiceTests : RepositoryTestBase
{
    private readonly CategoryService _service;
    private readonly CategoryRepository _repo;
    private readonly IMongoCollection<Category> _categories;

    public CategoryServiceTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new CategoryRepository(config);
        _service = new CategoryService(_repo);

        _categories = Fixture.Database.GetCollection<Category>("Categories");
        Fixture.Database.DropCollection("Categories");
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoCategories()
    {
        var result = await _service.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        await _repo.CreateAsync(new Category { Id = ObjectId.GenerateNewId().ToString(), Name = "A" });
        await _repo.CreateAsync(new Category { Id = ObjectId.GenerateNewId().ToString(), Name = "B" });

        var result = await _service.GetAllAsync();
        result.Should().HaveCount(2);
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdInvalid()
    {
        var result = await _service.GetByIdAsync("invalid-id");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.GetByIdAsync(id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategory_WhenExists()
    {
        var category = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Electronics"
        };

        await _repo.CreateAsync(category);

        var result = await _service.GetByIdAsync(category.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Electronics");
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenNameEmpty()
    {
        var result = await _service.CreateAsync("");
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCategory()
    {
        var result = await _service.CreateAsync("Books");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Books");

        var fetched = await _repo.GetByIdAsync(result.Id);
        fetched.Should().NotBeNull();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdInvalid()
    {
        var result = await _service.UpdateAsync("invalid-id", "NewName");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenCategoryNotFound()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.UpdateAsync(id, "NewName");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategory_WhenExists()
    {
        var category = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "OldName"
        };

        await _repo.CreateAsync(category);

        var updated = await _service.UpdateAsync(category.Id, "NewName");
        updated.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(category.Id);
        fetched!.Name.Should().Be("NewName");
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
    public async Task DeleteAsync_ShouldReturnFalse_WhenCategoryNotFound()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.DeleteAsync(id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCategory_WhenExists()
    {
        var category = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "DeleteMe"
        };

        await _repo.CreateAsync(category);

        var deleted = await _service.DeleteAsync(category.Id);
        deleted.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(category.Id);
        fetched.Should().BeNull();
    }
}

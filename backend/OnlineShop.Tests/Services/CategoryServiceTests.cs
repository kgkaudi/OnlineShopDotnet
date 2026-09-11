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
    // GET BY ID — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsNull()
    {
        var result = await _service.GetByIdAsync(null!);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsWhitespace()
    {
        var result = await _service.GetByIdAsync("   ");
        result.Should().BeNull();
    }

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
    // CREATE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenNameIsNull()
    {
        var result = await _service.CreateAsync(null!);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenNameIsWhitespace()
    {
        var result = await _service.CreateAsync("   ");
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenNameAlreadyExists()
    {
        await _repo.CreateAsync(new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Books"
        });

        var result = await _service.CreateAsync("Books");
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimName()
    {
        var result = await _service.CreateAsync("   Gadgets   ");
        result.Should().NotBeNull();
        result!.Name.Should().Be("Gadgets");
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
    // UPDATE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsNull()
    {
        var result = await _service.UpdateAsync(null!, "NewName");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsWhitespace()
    {
        var result = await _service.UpdateAsync("   ", "NewName");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdInvalid()
    {
        var result = await _service.UpdateAsync("invalid-id", "NewName");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNameIsNull()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.UpdateAsync(id, null!);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNameIsWhitespace()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.UpdateAsync(id, "   ");
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
    public async Task UpdateAsync_ShouldReturnFalse_WhenNameAlreadyExists()
    {
        var cat1 = new Category { Id = ObjectId.GenerateNewId().ToString(), Name = "A" };
        var cat2 = new Category { Id = ObjectId.GenerateNewId().ToString(), Name = "B" };

        await _repo.CreateAsync(cat1);
        await _repo.CreateAsync(cat2);

        var result = await _service.UpdateAsync(cat1.Id, "B");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNameIsUnchanged()
    {
        var category = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Same"
        };

        await _repo.CreateAsync(category);

        var result = await _service.UpdateAsync(category.Id, "Same");
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

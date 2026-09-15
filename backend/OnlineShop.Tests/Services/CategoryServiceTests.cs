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
    private readonly IMongoDatabase _testDatabase;

    public CategoryServiceTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var client = new MongoClient(
            fixture.Runner.ConnectionString
        );

        var databaseName = $"CatSvc_{Guid.NewGuid():N}";

        _testDatabase = client.GetDatabase(databaseName);

        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            databaseName
        );

        _repo = new CategoryRepository(config);
        _service = new CategoryService(_repo);
    }

    // ---------------------------------------------------------
    // TEST SETUP
    // ---------------------------------------------------------

    private void ClearCategories()
    {
        // The real collection name is "Category" (singular).
        //
        // We use DeleteMany instead of DropCollection because:
        // 1. Each test instance already has its own unique database.
        // 2. It avoids MongoDB database-name/drop issues.
        // 3. It works whether the collection already exists or not.
        var categories = _testDatabase.GetCollection<Category>("Category");

        categories.DeleteMany(
            FilterDefinition<Category>.Empty
        );
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoCategories()
    {
        ClearCategories();

        var result = await _service.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        ClearCategories();

        await _repo.CreateAsync(new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "A"
        });

        await _repo.CreateAsync(new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "B"
        });

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().ContainSingle(c => c.Name == "A");
        result.Should().ContainSingle(c => c.Name == "B");
    }

    // ---------------------------------------------------------
    // GET BY ID — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsNull()
    {
        ClearCategories();

        var result = await _service.GetByIdAsync(null!);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsWhitespace()
    {
        ClearCategories();

        var result = await _service.GetByIdAsync("   ");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdInvalid()
    {
        ClearCategories();

        var result = await _service.GetByIdAsync("invalid-id");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        ClearCategories();

        var id = ObjectId.GenerateNewId().ToString();

        var result = await _service.GetByIdAsync(id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategory_WhenExists()
    {
        ClearCategories();

        var category = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Electronics"
        };

        await _repo.CreateAsync(category);

        var result = await _service.GetByIdAsync(category.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
        result.Name.Should().Be("Electronics");
    }

    // ---------------------------------------------------------
    // CREATE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenNameIsNull()
    {
        ClearCategories();

        var result = await _service.CreateAsync(null!);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenNameIsWhitespace()
    {
        ClearCategories();

        var result = await _service.CreateAsync("   ");

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenNameAlreadyExists()
    {
        ClearCategories();

        await _repo.CreateAsync(new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Books"
        });

        var result = await _service.CreateAsync("Books");

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenNameAlreadyExists_IgnoringCase()
    {
        ClearCategories();

        await _repo.CreateAsync(new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Books"
        });

        var result = await _service.CreateAsync("books");

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimName()
    {
        ClearCategories();

        var result = await _service.CreateAsync("   Gadgets   ");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Gadgets");
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCategory()
    {
        ClearCategories();

        var result = await _service.CreateAsync("Books");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Books");
        result.Id.Should().NotBeNullOrWhiteSpace();

        var fetched = await _repo.GetByIdAsync(result.Id);

        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("Books");
    }

    // ---------------------------------------------------------
    // UPDATE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsNull()
    {
        ClearCategories();

        var result = await _service.UpdateAsync(null!, "NewName");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsWhitespace()
    {
        ClearCategories();

        var result = await _service.UpdateAsync("   ", "NewName");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdInvalid()
    {
        ClearCategories();

        var result = await _service.UpdateAsync("invalid-id", "NewName");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNameIsNull()
    {
        ClearCategories();

        var id = ObjectId.GenerateNewId().ToString();

        var result = await _service.UpdateAsync(id, null!);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNameIsWhitespace()
    {
        ClearCategories();

        var id = ObjectId.GenerateNewId().ToString();

        var result = await _service.UpdateAsync(id, "   ");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenCategoryNotFound()
    {
        ClearCategories();

        var id = ObjectId.GenerateNewId().ToString();

        var result = await _service.UpdateAsync(id, "NewName");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNameAlreadyExists()
    {
        ClearCategories();

        var cat1 = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "A"
        };

        var cat2 = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "B"
        };

        await _repo.CreateAsync(cat1);
        await _repo.CreateAsync(cat2);

        var result = await _service.UpdateAsync(cat1.Id, "B");

        result.Should().BeFalse();

        var unchanged = await _repo.GetByIdAsync(cat1.Id);

        unchanged.Should().NotBeNull();
        unchanged!.Name.Should().Be("A");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNameAlreadyExists_IgnoringCase()
    {
        ClearCategories();

        var cat1 = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "A"
        };

        var cat2 = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Books"
        };

        await _repo.CreateAsync(cat1);
        await _repo.CreateAsync(cat2);

        var result = await _service.UpdateAsync(cat1.Id, "books");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNameIsUnchanged()
    {
        ClearCategories();

        var category = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Same"
        };

        await _repo.CreateAsync(category);

        var result = await _service.UpdateAsync(
            category.Id,
            "Same"
        );

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNameIsUnchanged_IgnoringCase()
    {
        ClearCategories();

        var category = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Same"
        };

        await _repo.CreateAsync(category);

        var result = await _service.UpdateAsync(
            category.Id,
            "same"
        );

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldTrimName()
    {
        ClearCategories();

        var category = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "OldName"
        };

        await _repo.CreateAsync(category);

        var updated = await _service.UpdateAsync(
            category.Id,
            "   NewName   "
        );

        updated.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(category.Id);

        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("NewName");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategory_WhenExists()
    {
        ClearCategories();

        var category = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "OldName"
        };

        await _repo.CreateAsync(category);

        var updated = await _service.UpdateAsync(
            category.Id,
            "NewName"
        );

        updated.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(category.Id);

        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("NewName");
    }

    // ---------------------------------------------------------
    // DELETE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsNull()
    {
        ClearCategories();

        var result = await _service.DeleteAsync(null!);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsWhitespace()
    {
        ClearCategories();

        var result = await _service.DeleteAsync("   ");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdInvalid()
    {
        ClearCategories();

        var result = await _service.DeleteAsync("invalid-id");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenCategoryNotFound()
    {
        ClearCategories();

        var id = ObjectId.GenerateNewId().ToString();

        var result = await _service.DeleteAsync(id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCategory_WhenExists()
    {
        ClearCategories();

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

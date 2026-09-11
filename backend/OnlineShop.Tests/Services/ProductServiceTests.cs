using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using OnlineShop.Api.Services;
using Xunit;

public class ProductServiceTests : RepositoryTestBase
{
    private readonly ProductService _service;
    private readonly ProductRepository _repo;
    private readonly IMongoCollection<Product> _products;

    public ProductServiceTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new ProductRepository(config);
        _service = new ProductService(_repo);

        _products = Fixture.Database.GetCollection<Product>("Products");
        Fixture.Database.DropCollection("Products");
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoProducts()
    {
        var result = await _service.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProducts()
    {
        await _repo.CreateAsync(new Product { Id = ObjectId.GenerateNewId().ToString(), Name = "A", Price = 10 });
        await _repo.CreateAsync(new Product { Id = ObjectId.GenerateNewId().ToString(), Name = "B", Price = 20 });

        var result = await _service.GetAllAsync();
        result.Should().HaveCount(2);
    }

    // ---------------------------------------------------------
    // GET BY ID — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdNull()
    {
        var result = await _service.GetByIdAsync(null!);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdWhitespace()
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
    public async Task GetByIdAsync_ShouldReturnProduct_WhenExists()
    {
        var product = new Product
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Test",
            Price = 10
        };

        await _repo.CreateAsync(product);

        var result = await _service.GetByIdAsync(product.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
    }

    // ---------------------------------------------------------
    // CREATE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenProductIsNull()
    {
        var result = await _service.CreateAsync(null!);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenNameNull()
    {
        var result = await _service.CreateAsync(new Product { Name = null!, Price = 10 });
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenNameWhitespace()
    {
        var result = await _service.CreateAsync(new Product { Name = "   ", Price = 10 });
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenPriceZeroOrNegative()
    {
        var result1 = await _service.CreateAsync(new Product { Name = "X", Price = 0 });
        var result2 = await _service.CreateAsync(new Product { Name = "Y", Price = -5 });

        result1.Should().BeNull();
        result2.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenCategoryIdInvalid()
    {
        var result = await _service.CreateAsync(new Product
        {
            Name = "X",
            Price = 10,
            CategoryId = "invalid-id"
        });

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenStockNegative()
    {
        var result = await _service.CreateAsync(new Product
        {
            Name = "X",
            Price = 10,
            StockQuantity = -1
        });

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimName()
    {
        var created = await _service.CreateAsync(new Product
        {
            Name = "   New   ",
            Price = 10
        });

        created.Should().NotBeNull();
        created!.Name.Should().Be("New");
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateId_WhenMissing()
    {
        var product = new Product
        {
            Name = "New",
            Price = 10
        };

        var created = await _service.CreateAsync(product);

        created.Should().NotBeNull();
        created!.Id.Should().NotBeNull();
    }

    // ---------------------------------------------------------
    // UPDATE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenProductIsNull()
    {
        var result = await _service.UpdateAsync(null!);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdNull()
    {
        var product = new Product { Id = null!, Name = "X", Price = 10 };
        var result = await _service.UpdateAsync(product);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdWhitespace()
    {
        var product = new Product { Id = "   ", Name = "X", Price = 10 };
        var result = await _service.UpdateAsync(product);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdInvalid()
    {
        var product = new Product { Id = "invalid-id", Name = "X", Price = 10 };
        var result = await _service.UpdateAsync(product);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenProductNotFound()
    {
        var product = new Product
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Missing",
            Price = 10
        };

        var result = await _service.UpdateAsync(product);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNameNull()
    {
        var product = new Product
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = null!,
            Price = 10
        };

        await _repo.CreateAsync(new Product
        {
            Id = product.Id,
            Name = "Old",
            Price = 10
        });

        var result = await _service.UpdateAsync(product);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNameWhitespace()
    {
        var product = new Product
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "   ",
            Price = 10
        };

        await _repo.CreateAsync(new Product
        {
            Id = product.Id,
            Name = "Old",
            Price = 10
        });

        var result = await _service.UpdateAsync(product);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenPriceZeroOrNegative()
    {
        var product = new Product
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "X",
            Price = 0
        };

        await _repo.CreateAsync(new Product
        {
            Id = product.Id,
            Name = "Old",
            Price = 10
        });

        var result = await _service.UpdateAsync(product);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenCategoryIdInvalid()
    {
        var product = new Product
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "X",
            Price = 10,
            CategoryId = "invalid-id"
        };

        await _repo.CreateAsync(new Product
        {
            Id = product.Id,
            Name = "Old",
            Price = 10
        });

        var result = await _service.UpdateAsync(product);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenStockNegative()
    {
        var product = new Product
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "X",
            Price = 10,
            StockQuantity = -1
        };

        await _repo.CreateAsync(new Product
        {
            Id = product.Id,
            Name = "Old",
            Price = 10
        });

        var result = await _service.UpdateAsync(product);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdate_WhenExists()
    {
        var product = new Product
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Old",
            Price = 10
        };

        await _repo.CreateAsync(product);

        product.Name = "Updated";

        var updated = await _service.UpdateAsync(product);
        updated.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(product.Id);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("Updated");
    }

    // ---------------------------------------------------------
    // DELETE — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdNull()
    {
        var result = await _service.DeleteAsync(null!);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdWhitespace()
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
    public async Task DeleteAsync_ShouldReturnFalse_WhenProductNotFound()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.DeleteAsync(id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDelete_WhenExists()
    {
        var product = new Product
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "DeleteMe",
            Price = 10
        };

        await _repo.CreateAsync(product);

        var deleted = await _service.DeleteAsync(product.Id);
        deleted.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(product.Id);
        fetched.Should().BeNull();
    }

    // ---------------------------------------------------------
    // SEARCH — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task SearchAsync_ShouldReturnEmpty_WhenKeywordNull()
    {
        var result = await _service.SearchAsync(null, null, null, null, null, null, false);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmpty_WhenKeywordWhitespace()
    {
        var result = await _service.SearchAsync("   ", null, null, null, null, null, false);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByKeyword()
    {
        await _repo.CreateAsync(new Product { Id = ObjectId.GenerateNewId().ToString(), Name = "Laptop", Price = 100 });
        await _repo.CreateAsync(new Product { Id = ObjectId.GenerateNewId().ToString(), Name = "Phone", Price = 50 });

        var result = await _service.SearchAsync("lap", null, null, null, null, null, false);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByCategory()
    {
        var catA = ObjectId.GenerateNewId().ToString();
        var catB = ObjectId.GenerateNewId().ToString();

        await _repo.CreateAsync(new Product { Id = ObjectId.GenerateNewId().ToString(), Name = "A", CategoryId = catA, Price = 10 });
        await _repo.CreateAsync(new Product { Id = ObjectId.GenerateNewId().ToString(), Name = "B", CategoryId = catB, Price = 20 });

        var result = await _service.SearchAsync(null, catA, null, null, null, null, false);

        result.Should().HaveCount(1);
        result[0].CategoryId.Should().Be(catA);
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByPriceRange()
    {
        await _repo.CreateAsync(new Product { Id = ObjectId.GenerateNewId().ToString(), Name = "Cheap", Price = 10 });
        await _repo.CreateAsync(new Product { Id = ObjectId.GenerateNewId().ToString(), Name = "Expensive", Price = 100 });

        var result = await _service.SearchAsync(null, null, 20, 200, null, null, false);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Expensive");
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByStock()
    {
        await _repo.CreateAsync(new Product { Id = ObjectId.GenerateNewId().ToString(), Name = "InStock", Price = 10, StockQuantity = 5 });
        await _repo.CreateAsync(new Product { Id = ObjectId.GenerateNewId().ToString(), Name = "OutOfStock", Price = 10, StockQuantity = 0 });

        var result = await _service.SearchAsync(null, null, null, null, true, null, false);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("InStock");
    }

    [Fact]
    public async Task SearchAsync_ShouldSortDescending()
    {
        await _repo.CreateAsync(new Product { Id = ObjectId.GenerateNewId().ToString(), Name = "Cheap", Price = 10 });
        await _repo.CreateAsync(new Product { Id = ObjectId.GenerateNewId().ToString(), Name = "Expensive", Price = 100 });

        var result = await _service.SearchAsync(null, null, null, null, null, "Price", true);

        result.First().Name.Should().Be("Expensive");
    }
}

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
    // GET BY ID
    // ---------------------------------------------------------

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
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldGenerateId_WhenMissing()
    {
        var product = new Product
        {
            Name = "New",
            Price = 10
        };

        var created = await _service.CreateAsync(product);

        created.Id.Should().NotBeNull();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

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
        fetched!.Name.Should().Be("Updated");
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
    // SEARCH
    // ---------------------------------------------------------

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

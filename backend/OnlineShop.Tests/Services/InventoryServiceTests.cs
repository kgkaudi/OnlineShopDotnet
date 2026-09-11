using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using OnlineShop.Api.Services;
using Xunit;

public class InventoryServiceTests : RepositoryTestBase
{
    private readonly InventoryService _service;
    private readonly InventoryRepository _repo;
    private readonly IMongoCollection<Product> _products;

    public InventoryServiceTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new InventoryRepository(config);
        _service = new InventoryService(_repo);

        _products = Fixture.Database.GetCollection<Product>("Products");
        Fixture.Database.DropCollection("Products");
    }

    // ---------------------------------------------------------
    // RESTOCK — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task RestockAsync_ShouldReturnFalse_WhenProductIdIsNull()
    {
        var result = await _service.RestockAsync(null!, 10);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RestockAsync_ShouldReturnFalse_WhenProductIdIsWhitespace()
    {
        var result = await _service.RestockAsync("   ", 10);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RestockAsync_ShouldReturnFalse_WhenProductIdInvalid()
    {
        var result = await _service.RestockAsync("invalid-id", 10);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RestockAsync_ShouldReturnFalse_WhenAmountZero()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.RestockAsync(id, 0);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RestockAsync_ShouldReturnFalse_WhenAmountNegative()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.RestockAsync(id, -5);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RestockAsync_ShouldReturnFalse_WhenProductNotFound()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.RestockAsync(id, 10);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RestockAsync_ShouldIncreaseStock_WhenProductExists()
    {
        var product = new Product
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Test",
            Price = 10,
            StockQuantity = 5
        };

        await _products.InsertOneAsync(product);

        var result = await _service.RestockAsync(product.Id, 10);
        result.Should().BeTrue();

        var fetched = await _products.Find(p => p.Id == product.Id).FirstOrDefaultAsync();
        fetched!.StockQuantity.Should().Be(15);
    }

    // ---------------------------------------------------------
    // REDUCE STOCK — EDGE CASES
    // ---------------------------------------------------------

    [Fact]
    public async Task ReduceStockAsync_ShouldReturnFalse_WhenProductIdIsNull()
    {
        var result = await _service.ReduceStockAsync(null!, 10);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReduceStockAsync_ShouldReturnFalse_WhenProductIdIsWhitespace()
    {
        var result = await _service.ReduceStockAsync("   ", 10);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReduceStockAsync_ShouldReturnFalse_WhenProductIdInvalid()
    {
        var result = await _service.ReduceStockAsync("invalid-id", 10);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReduceStockAsync_ShouldReturnFalse_WhenAmountZero()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.ReduceStockAsync(id, 0);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReduceStockAsync_ShouldReturnFalse_WhenAmountNegative()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.ReduceStockAsync(id, -5);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReduceStockAsync_ShouldReturnFalse_WhenProductNotFound()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _service.ReduceStockAsync(id, 10);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReduceStockAsync_ShouldReturnFalse_WhenNotEnoughStock()
    {
        var product = new Product
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Test",
            Price = 10,
            StockQuantity = 5
        };

        await _products.InsertOneAsync(product);

        var result = await _service.ReduceStockAsync(product.Id, 10);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReduceStockAsync_ShouldDecreaseStock_WhenEnoughStock()
    {
        var product = new Product
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "Test",
            Price = 10,
            StockQuantity = 20
        };

        await _products.InsertOneAsync(product);

        var result = await _service.ReduceStockAsync(product.Id, 5);
        result.Should().BeTrue();

        var fetched = await _products.Find(p => p.Id == product.Id).FirstOrDefaultAsync();
        fetched!.StockQuantity.Should().Be(15);
    }
}

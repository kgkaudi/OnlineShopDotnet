using FluentAssertions;
using MongoDB.Driver;
using MongoDB.Bson;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using Xunit;

public class InventoryRepositoryTests : RepositoryTestBase
{
    private readonly InventoryRepository _repo;
    private readonly IMongoCollection<Product> _products;

    public InventoryRepositoryTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new InventoryRepository(config);
        _products = Fixture.Database.GetCollection<Product>("Products");

        Fixture.Database.DropCollection("Products");
    }

    // ---------------------------------------------------------
    // INCREASE STOCK
    // ---------------------------------------------------------

    [Fact]
    public async Task IncreaseStockAsync_ShouldReturnFalse_WhenIdIsNull()
    {
        var result = await _repo.IncreaseStockAsync(null!, 5);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IncreaseStockAsync_ShouldReturnFalse_WhenIdIsWhitespace()
    {
        var result = await _repo.IncreaseStockAsync("   ", 5);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IncreaseStockAsync_ShouldReturnFalse_WhenIdIsInvalid()
    {
        var result = await _repo.IncreaseStockAsync("invalid-id", 5);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IncreaseStockAsync_ShouldReturnFalse_WhenProductDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _repo.IncreaseStockAsync(id, 5);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IncreaseStockAsync_ShouldReturnFalse_WhenAmountIsZero()
    {
        var product = new Product { Name = "Test", Price = 10, StockQuantity = 5 };
        await _products.InsertOneAsync(product);

        var result = await _repo.IncreaseStockAsync(product.Id, 0);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IncreaseStockAsync_ShouldReturnFalse_WhenAmountIsNegative()
    {
        var product = new Product { Name = "Test", Price = 10, StockQuantity = 5 };
        await _products.InsertOneAsync(product);

        var result = await _repo.IncreaseStockAsync(product.Id, -3);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IncreaseStockAsync_ShouldIncreaseStock_WhenProductExists()
    {
        var product = new Product
        {
            Name = "Test",
            Price = 10,
            StockQuantity = 5
        };

        await _products.InsertOneAsync(product);

        var updated = await _repo.IncreaseStockAsync(product.Id, 3);
        updated.Should().BeTrue();

        var fetched = await _products.Find(p => p.Id == product.Id).FirstOrDefaultAsync();
        fetched!.StockQuantity.Should().Be(8);
    }

    [Fact]
    public async Task IncreaseStockAsync_ShouldIncreaseStock_WhenStockIsZero()
    {
        var product = new Product { Name = "Zero", Price = 10, StockQuantity = 0 };
        await _products.InsertOneAsync(product);

        var updated = await _repo.IncreaseStockAsync(product.Id, 10);
        updated.Should().BeTrue();

        var fetched = await _products.Find(p => p.Id == product.Id).FirstOrDefaultAsync();
        fetched!.StockQuantity.Should().Be(10);
    }

    [Fact]
    public async Task IncreaseStockAsync_ShouldNotAffectOtherProducts()
    {
        var p1 = new Product { Name = "A", Price = 10, StockQuantity = 5 };
        var p2 = new Product { Name = "B", Price = 10, StockQuantity = 7 };

        await _products.InsertOneAsync(p1);
        await _products.InsertOneAsync(p2);

        await _repo.IncreaseStockAsync(p1.Id, 3);

        var fetched2 = await _products.Find(p => p.Id == p2.Id).FirstOrDefaultAsync();
        fetched2!.StockQuantity.Should().Be(7);
    }

    // ---------------------------------------------------------
    // DECREASE STOCK
    // ---------------------------------------------------------

    [Fact]
    public async Task DecreaseStockAsync_ShouldReturnFalse_WhenIdIsNull()
    {
        var result = await _repo.DecreaseStockAsync(null!, 5);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DecreaseStockAsync_ShouldReturnFalse_WhenIdIsWhitespace()
    {
        var result = await _repo.DecreaseStockAsync("   ", 5);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DecreaseStockAsync_ShouldReturnFalse_WhenIdIsInvalid()
    {
        var result = await _repo.DecreaseStockAsync("invalid-id", 5);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DecreaseStockAsync_ShouldReturnFalse_WhenProductDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _repo.DecreaseStockAsync(id, 5);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DecreaseStockAsync_ShouldReturnFalse_WhenAmountIsZero()
    {
        var product = new Product { Name = "Test", Price = 10, StockQuantity = 5 };
        await _products.InsertOneAsync(product);

        var result = await _repo.DecreaseStockAsync(product.Id, 0);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DecreaseStockAsync_ShouldReturnFalse_WhenAmountIsNegative()
    {
        var product = new Product { Name = "Test", Price = 10, StockQuantity = 5 };
        await _products.InsertOneAsync(product);

        var result = await _repo.DecreaseStockAsync(product.Id, -3);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DecreaseStockAsync_ShouldReturnFalse_WhenStockIsTooLow()
    {
        var product = new Product
        {
            Name = "Test",
            Price = 10,
            StockQuantity = 2
        };

        await _products.InsertOneAsync(product);

        var result = await _repo.DecreaseStockAsync(product.Id, 5);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DecreaseStockAsync_ShouldDecreaseStock_WhenEnoughStockExists()
    {
        var product = new Product
        {
            Name = "Test",
            Price = 10,
            StockQuantity = 10
        };

        await _products.InsertOneAsync(product);

        var updated = await _repo.DecreaseStockAsync(product.Id, 4);
        updated.Should().BeTrue();

        var fetched = await _products.Find(p => p.Id == product.Id).FirstOrDefaultAsync();
        fetched!.StockQuantity.Should().Be(6);
    }

    [Fact]
    public async Task DecreaseStockAsync_ShouldReturnTrue_WhenStockBecomesZero()
    {
        var product = new Product
        {
            Name = "Test",
            Price = 10,
            StockQuantity = 3
        };

        await _products.InsertOneAsync(product);

        var updated = await _repo.DecreaseStockAsync(product.Id, 3);
        updated.Should().BeTrue();

        var fetched = await _products.Find(p => p.Id == product.Id).FirstOrDefaultAsync();
        fetched!.StockQuantity.Should().Be(0);
    }

    [Fact]
    public async Task DecreaseStockAsync_ShouldNotAffectOtherProducts()
    {
        var p1 = new Product { Name = "A", Price = 10, StockQuantity = 10 };
        var p2 = new Product { Name = "B", Price = 10, StockQuantity = 7 };

        await _products.InsertOneAsync(p1);
        await _products.InsertOneAsync(p2);

        await _repo.DecreaseStockAsync(p1.Id, 4);

        var fetched2 = await _products.Find(p => p.Id == p2.Id).FirstOrDefaultAsync();
        fetched2!.StockQuantity.Should().Be(7);
    }
}

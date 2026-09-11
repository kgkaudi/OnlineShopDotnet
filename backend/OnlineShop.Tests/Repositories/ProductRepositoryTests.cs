using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using Xunit;

public class ProductRepositoryTests : RepositoryTestBase
{
    private readonly ProductRepository _repo;
    private readonly IMongoCollection<Product> _products;

    public ProductRepositoryTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new ProductRepository(config);
        _products = Fixture.Database.GetCollection<Product>("Products");

        Fixture.Database.DropCollection("Products");
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenProductIsNull()
    {
        Func<Task> act = async () => await _repo.CreateAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNameIsMissing()
    {
        var product = new Product { Name = null!, Price = 10 };

        Func<Task> act = async () => await _repo.CreateAsync(product);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenPriceIsZeroOrNegative()
    {
        var product = new Product { Name = "X", Price = 0 };

        Func<Task> act = async () => await _repo.CreateAsync(product);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldInsertProduct()
    {
        var product = new Product
        {
            Name = "Test Product",
            Price = 10,
            CategoryId = "cat1",
            StockQuantity = 5
        };

        await _repo.CreateAsync(product);

        var fetched = await _repo.GetByIdAsync(product.Id);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task CreateAsync_ShouldInsertProduct_WithMissingOptionalFields()
    {
        var product = new Product
        {
            Name = "Minimal Product",
            Price = 1
        };

        await _repo.CreateAsync(product);

        var fetched = await _repo.GetByIdAsync(product.Id);
        fetched.Should().NotBeNull();
        fetched!.Description.Should().BeNull();
        fetched.CategoryId.Should().BeNull();
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
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repo.GetByIdAsync(ObjectId.GenerateNewId().ToString());
        result.Should().BeNull();
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoProducts()
    {
        var result = await _repo.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProducts()
    {
        await _repo.CreateAsync(new Product { Name = "A", Price = 1 });
        await _repo.CreateAsync(new Product { Name = "B", Price = 2 });

        var result = await _repo.GetAllAsync();
        result.Should().HaveCount(2);
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenProductIsNull()
    {
        var result = await _repo.UpdateAsync(null!);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsMissing()
    {
        var product = new Product { Id = null!, Name = "X", Price = 1 };
        var result = await _repo.UpdateAsync(product);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsWhitespace()
    {
        var product = new Product { Id = "   ", Name = "X", Price = 1 };
        var result = await _repo.UpdateAsync(product);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenIdIsInvalid()
    {
        var product = new Product { Id = "invalid-id", Name = "X", Price = 1 };
        var result = await _repo.UpdateAsync(product);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNameIsMissing()
    {
        var product = new Product { Name = "Old", Price = 1 };
        await _repo.CreateAsync(product);

        product.Name = null!;
        var result = await _repo.UpdateAsync(product);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenPriceIsZeroOrNegative()
    {
        var product = new Product { Name = "Old", Price = 1 };
        await _repo.CreateAsync(product);

        product.Price = 0;
        var result = await _repo.UpdateAsync(product);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenProductDoesNotExist()
    {
        var product = new Product { Id = ObjectId.GenerateNewId().ToString(), Name = "X", Price = 1 };
        var result = await _repo.UpdateAsync(product);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingProduct()
    {
        var product = new Product { Name = "Old", Price = 1 };
        await _repo.CreateAsync(product);

        product.Name = "Updated";
        var updated = await _repo.UpdateAsync(product);

        updated.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(product.Id);
        fetched!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotAffectOtherProducts()
    {
        var p1 = new Product { Name = "A", Price = 1 };
        var p2 = new Product { Name = "B", Price = 2 };

        await _repo.CreateAsync(p1);
        await _repo.CreateAsync(p2);

        p1.Name = "Updated A";
        await _repo.UpdateAsync(p1);

        var fetched2 = await _repo.GetByIdAsync(p2.Id);
        fetched2!.Name.Should().Be("B");
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
    public async Task DeleteAsync_ShouldReturnFalse_WhenProductDoesNotExist()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var result = await _repo.DeleteAsync(id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteExistingProduct()
    {
        var product = new Product { Name = "Delete Me", Price = 1 };
        await _repo.CreateAsync(product);

        var deleted = await _repo.DeleteAsync(product.Id);
        deleted.Should().BeTrue();

        var fetched = await _repo.GetByIdAsync(product.Id);
        fetched.Should().BeNull();
    }

    // ---------------------------------------------------------
    // SEARCH
    // ---------------------------------------------------------

    [Fact]
    public async Task SearchAsync_ShouldReturnAll_WhenNoFilters()
    {
        await _repo.CreateAsync(new Product { Name = "A", Price = 1 });
        await _repo.CreateAsync(new Product { Name = "B", Price = 2 });

        var result = await _repo.SearchAsync(null, null, null, null, null, null, false);
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByKeyword()
    {
        await _repo.CreateAsync(new Product { Name = "Red Shoes", Price = 10 });
        await _repo.CreateAsync(new Product { Name = "Blue Shirt", Price = 20 });

        var result = await _repo.SearchAsync("Red", null, null, null, null, null, false);
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Red Shoes");
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByCategory()
    {
        await _repo.CreateAsync(new Product { Name = "A", Price = 1, CategoryId = "cat1" });
        await _repo.CreateAsync(new Product { Name = "B", Price = 2, CategoryId = "cat2" });

        var result = await _repo.SearchAsync(null, "cat1", null, null, null, null, false);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByPriceRange()
    {
        await _repo.CreateAsync(new Product { Name = "Cheap", Price = 5 });
        await _repo.CreateAsync(new Product { Name = "Mid", Price = 15 });
        await _repo.CreateAsync(new Product { Name = "Expensive", Price = 50 });

        var result = await _repo.SearchAsync(null, null, 10, 20, null, null, false);
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Mid");
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByInStock()
    {
        await _repo.CreateAsync(new Product { Name = "In Stock", Price = 10, StockQuantity = 5 });
        await _repo.CreateAsync(new Product { Name = "Out of Stock", Price = 10, StockQuantity = 0 });

        var result = await _repo.SearchAsync(null, null, null, null, true, null, false);
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("In Stock");
    }

    [Fact]
    public async Task SearchAsync_ShouldSortAscending()
    {
        await _repo.CreateAsync(new Product { Name = "B", Price = 20 });
        await _repo.CreateAsync(new Product { Name = "A", Price = 10 });

        var result = await _repo.SearchAsync(null, null, null, null, null, "Name", false);
        result[0].Name.Should().Be("A");
    }

    [Fact]
    public async Task SearchAsync_ShouldSortDescending()
    {
        await _repo.CreateAsync(new Product { Name = "A", Price = 10 });
        await _repo.CreateAsync(new Product { Name = "B", Price = 20 });

        var result = await _repo.SearchAsync(null, null, null, null, null, "Name", true);
        result[0].Name.Should().Be("B");
    }

    [Fact]
    public async Task SearchAsync_ShouldApplyMultipleFiltersTogether()
    {
        await _repo.CreateAsync(new Product
        {
            Name = "Gaming Laptop",
            Price = 1500,
            CategoryId = "electronics",
            StockQuantity = 10
        });

        await _repo.CreateAsync(new Product
        {
            Name = "Office Laptop",
            Price = 900,
            CategoryId = "electronics",
            StockQuantity = 0
        });

        var result = await _repo.SearchAsync(
            "Laptop",
            "electronics",
            1000,
            2000,
            true,
            "Price",
            false
        );

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Gaming Laptop");
    }
}

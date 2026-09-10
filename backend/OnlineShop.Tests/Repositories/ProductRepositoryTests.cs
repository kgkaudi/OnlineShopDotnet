using FluentAssertions;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;
using Xunit;

public class ProductRepositoryTests : RepositoryTestBase
{
    private readonly ProductRepository _repo;

    public ProductRepositoryTests(MongoTestFixture fixture)
        : base(fixture)
    {
        var config = TestConfiguration.Create(
            fixture.Runner.ConnectionString,
            "OnlineShop_TestDb"
        );

        _repo = new ProductRepository(config);
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

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
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repo.GetByIdAsync("non-existing-id");
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
    public async Task UpdateAsync_ShouldReturnFalse_WhenProductDoesNotExist()
    {
        var product = new Product { Id = "missing", Name = "X", Price = 1 };

        var result = await _repo.UpdateAsync(product);
        result.Should().BeFalse();
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

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

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenProductDoesNotExist()
    {
        var result = await _repo.DeleteAsync("missing-id");
        result.Should().BeFalse();
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

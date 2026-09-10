using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Controllers;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;
using Xunit;
using MongoDB.Bson;
using System.Security.Claims;

public class ProductsControllerTests
{
    private ProductsController CreateController(IProductService service, bool isAdmin = false)
    {
        var controller = new ProductsController(service);

        var httpContext = new DefaultHttpContext();

        if (isAdmin)
        {
            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Role, "Admin")
                }, "test")
            );
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        var controller = CreateController(new FakeProductService());

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetById_ShouldReturnBadRequest_WhenIdInvalid()
    {
        var controller = CreateController(new FakeProductService());

        var result = await controller.GetById("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(new FakeProductService());

        var id = ObjectId.GenerateNewId().ToString();
        var result = await controller.GetById(id);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenExists()
    {
        var service = new FakeProductService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddProduct(id, "Test", 10);

        var controller = CreateController(service);

        var result = await controller.GetById(id);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // SEARCH
    // ---------------------------------------------------------

    [Fact]
    public async Task Search_ShouldReturnBadRequest_WhenCategoryIdInvalid()
    {
        var controller = CreateController(new FakeProductService());

        var result = await controller.Search(null, "invalid-id", null, null, null, null, false);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Search_ShouldReturnBadRequest_WhenPriceNegative()
    {
        var controller = CreateController(new FakeProductService());

        var result = await controller.Search(null, null, -1, null, null, null, false);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenNameMissing()
    {
        var controller = CreateController(new FakeProductService(), isAdmin: true);

        var result = await controller.Create(new Product { Name = "", Price = 10 });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenPriceInvalid()
    {
        var controller = CreateController(new FakeProductService(), isAdmin: true);

        var result = await controller.Create(new Product { Name = "Test", Price = 0 });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenValid()
    {
        var controller = CreateController(new FakeProductService(), isAdmin: true);

        var result = await controller.Create(new Product { Name = "New", Price = 10 });

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenIdInvalid()
    {
        var controller = CreateController(new FakeProductService(), isAdmin: true);

        var result = await controller.Update("invalid-id", new Product { Name = "Test", Price = 10 });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(new FakeProductService(), isAdmin: true);

        var id = ObjectId.GenerateNewId().ToString();
        var result = await controller.Update(id, new Product { Name = "Test", Price = 10 });

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenValid()
    {
        var service = new FakeProductService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddProduct(id, "Old", 10);

        var controller = CreateController(service, isAdmin: true);

        var result = await controller.Update(id, new Product { Name = "Updated", Price = 20 });

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenIdInvalid()
    {
        var controller = CreateController(new FakeProductService(), isAdmin: true);

        var result = await controller.Delete("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(new FakeProductService(), isAdmin: true);

        var id = ObjectId.GenerateNewId().ToString();
        var result = await controller.Delete(id);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenValid()
    {
        var service = new FakeProductService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddProduct(id, "ToDelete", 10);

        var controller = CreateController(service, isAdmin: true);

        var result = await controller.Delete(id);

        result.Should().BeOfType<OkObjectResult>();
    }
}

// ---------------------------------------------------------
// FAKE SERVICE FOR TESTING
// ---------------------------------------------------------

public class FakeProductService : IProductService
{
    private readonly Dictionary<string, Product> _store = new();

    public void AddProduct(string id, string name, decimal price)
    {
        _store[id] = new Product { Id = id, Name = name, Price = price };
    }

    public Task<List<Product>> GetAllAsync()
        => Task.FromResult(_store.Values.ToList());

    public Task<Product?> GetByIdAsync(string id)
        => Task.FromResult(_store.ContainsKey(id) ? _store[id] : null);

    public Task<List<Product>> SearchAsync(
        string? keyword,
        string? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        bool? inStock,
        string? sortBy,
        bool descending
    )
        => Task.FromResult(_store.Values.ToList());

    public Task<Product> CreateAsync(Product product)
    {
        product.Id = ObjectId.GenerateNewId().ToString();
        _store[product.Id] = product;
        return Task.FromResult(product);
    }

    public Task<bool> UpdateAsync(Product product)
    {
        if (!_store.ContainsKey(product.Id))
            return Task.FromResult(false);

        _store[product.Id] = product;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(string id)
        => Task.FromResult(_store.Remove(id));
}

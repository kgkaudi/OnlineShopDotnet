using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Controllers;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;
using Xunit;
using MongoDB.Bson;
using System.Security.Claims;

public class CategoriesControllerTests
{
    private CategoriesController CreateController(ICategoryService service, bool isAdmin = false)
    {
        var controller = new CategoriesController(service);

        var httpContext = new DefaultHttpContext();

        if (isAdmin)
        {
            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(new[]
                {
                    new Claim("role", "Admin")
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
        var controller = CreateController(new FakeCategoryService());

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetById_ShouldReturnBadRequest_WhenIdInvalid()
    {
        var controller = CreateController(new FakeCategoryService());

        var result = await controller.GetById("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenCategoryMissing()
    {
        var controller = CreateController(new FakeCategoryService());

        var id = ObjectId.GenerateNewId().ToString();
        var result = await controller.GetById(id);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenCategoryExists()
    {
        var service = new FakeCategoryService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddCategory(id, "Test");

        var controller = CreateController(service);

        var result = await controller.GetById(id);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenNameMissing()
    {
        var controller = CreateController(new FakeCategoryService(), isAdmin: true);

        var result = await controller.Create(new Category { Name = "" });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenValid()
    {
        var controller = CreateController(new FakeCategoryService(), isAdmin: true);

        var result = await controller.Create(new Category { Name = "NewCat" });

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenIdInvalid()
    {
        var controller = CreateController(new FakeCategoryService(), isAdmin: true);

        var result = await controller.Update("invalid-id", new Category { Name = "Test" });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenNameMissing()
    {
        var controller = CreateController(new FakeCategoryService(), isAdmin: true);

        var id = ObjectId.GenerateNewId().ToString();
        var result = await controller.Update(id, new Category { Name = "" });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenCategoryMissing()
    {
        var controller = CreateController(new FakeCategoryService(), isAdmin: true);

        var id = ObjectId.GenerateNewId().ToString();
        var result = await controller.Update(id, new Category { Name = "Test" });

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenValid()
    {
        var service = new FakeCategoryService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddCategory(id, "Old");

        var controller = CreateController(service, isAdmin: true);

        var result = await controller.Update(id, new Category { Name = "Updated" });

        result.Should().BeOfType<NoContentResult>();
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenIdInvalid()
    {
        var controller = CreateController(new FakeCategoryService(), isAdmin: true);

        var result = await controller.Delete("invalid-id");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenCategoryMissing()
    {
        var controller = CreateController(new FakeCategoryService(), isAdmin: true);

        var id = ObjectId.GenerateNewId().ToString();
        var result = await controller.Delete(id);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenValid()
    {
        var service = new FakeCategoryService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddCategory(id, "ToDelete");

        var controller = CreateController(service, isAdmin: true);

        var result = await controller.Delete(id);

        result.Should().BeOfType<NoContentResult>();
    }
}

// ---------------------------------------------------------
// FAKE SERVICE FOR TESTING
// ---------------------------------------------------------

public class FakeCategoryService : ICategoryService
{
    private readonly Dictionary<string, Category> _store = new();

    public void AddCategory(string id, string name)
    {
        _store[id] = new Category { Id = id, Name = name };
    }

    public Task<List<Category>> GetAllAsync()
        => Task.FromResult(_store.Values.ToList());

    public Task<Category?> GetByIdAsync(string id)
        => Task.FromResult(_store.ContainsKey(id) ? _store[id] : null);

    public Task<Category?> CreateAsync(string name)
    {
        var id = ObjectId.GenerateNewId().ToString();
        var cat = new Category { Id = id, Name = name };
        _store[id] = cat;
        return Task.FromResult<Category?>(cat);
    }

    public Task<bool> UpdateAsync(string id, string name)
    {
        if (!_store.ContainsKey(id))
            return Task.FromResult(false);

        _store[id].Name = name;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(string id)
    {
        return Task.FromResult(_store.Remove(id));
    }
}

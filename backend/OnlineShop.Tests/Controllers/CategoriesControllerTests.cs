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
        var controller = CreateController(new FakeCategoryService());

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyList_WhenNoCategories()
    {
        var controller = CreateController(new FakeCategoryService());

        var result = await controller.GetAll();

        var list = result.As<OkObjectResult>().Value as List<Category>;
        list.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_ShouldReturnList_WhenCategoriesExist()
    {
        var service = new FakeCategoryService();
        service.AddCategory(ObjectId.GenerateNewId().ToString(), "A");
        service.AddCategory(ObjectId.GenerateNewId().ToString(), "B");

        var controller = CreateController(service);

        var result = await controller.GetAll();

        var list = result.As<OkObjectResult>().Value as List<Category>;
        list.Should().HaveCount(2);
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
    public async Task GetById_ShouldReturnBadRequest_WhenIdNull()
    {
        var controller = CreateController(new FakeCategoryService());

        var result = await controller.GetById(null!);

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
    public async Task Create_ShouldReturnUnauthorized_WhenNotAdmin()
    {
        var controller = CreateController(new FakeCategoryService(), isAdmin: false);

        var result = await controller.Create(new Category { Name = "X" });

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenCategoryNull()
    {
        var controller = CreateController(new FakeCategoryService(), isAdmin: true);

        var result = await controller.Create(null!);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenNameMissing()
    {
        var controller = CreateController(new FakeCategoryService(), isAdmin: true);

        var result = await controller.Create(new Category { Name = "" });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenNameWhitespace()
    {
        var controller = CreateController(new FakeCategoryService(), isAdmin: true);

        var result = await controller.Create(new Category { Name = "   " });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenValid()
    {
        var controller = CreateController(new FakeCategoryService(), isAdmin: true);

        var result = await controller.Create(new Category { Name = "NewCat" });

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDuplicateName()
    {
        var service = new FakeCategoryService();
        service.AddCategory(ObjectId.GenerateNewId().ToString(), "Dup");

        var controller = CreateController(service, isAdmin: true);

        var result = await controller.Create(new Category { Name = "Dup" });

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task Update_ShouldReturnUnauthorized_WhenNotAdmin()
    {
        var controller = CreateController(new FakeCategoryService(), isAdmin: false);

        var result = await controller.Update(ObjectId.GenerateNewId().ToString(), new Category { Name = "X" });

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenIdInvalid()
    {
        var controller = CreateController(new FakeCategoryService(), isAdmin: true);

        var result = await controller.Update("invalid-id", new Category { Name = "Test" });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenCategoryNull()
    {
        var controller = CreateController(new FakeCategoryService(), isAdmin: true);

        var result = await controller.Update(ObjectId.GenerateNewId().ToString(), null!);

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

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenUpdatingTwice()
    {
        var service = new FakeCategoryService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddCategory(id, "Old");

        var controller = CreateController(service, isAdmin: true);

        var first = await controller.Update(id, new Category { Name = "Updated" });
        var second = await controller.Update(id, new Category { Name = "UpdatedAgain" });

        first.Should().BeOfType<NoContentResult>();
        second.Should().BeOfType<NotFoundObjectResult>();
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task Delete_ShouldReturnUnauthorized_WhenNotAdmin()
    {
        var controller = CreateController(new FakeCategoryService(), isAdmin: false);

        var result = await controller.Delete(ObjectId.GenerateNewId().ToString());

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

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

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenDeletingTwice()
    {
        var service = new FakeCategoryService();
        var id = ObjectId.GenerateNewId().ToString();
        service.AddCategory(id, "ToDelete");

        var controller = CreateController(service, isAdmin: true);

        var first = await controller.Delete(id);
        var second = await controller.Delete(id);

        first.Should().BeOfType<NoContentResult>();
        second.Should().BeOfType<NotFoundObjectResult>();
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
        if (string.IsNullOrWhiteSpace(name))
            return Task.FromResult<Category?>(null);

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
        => Task.FromResult(_store.Remove(id));
}

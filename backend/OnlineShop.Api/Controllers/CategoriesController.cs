using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    // GET ALL (Public)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _service.GetAllAsync();
        return Ok(categories);
    }

    // GET BY ID (Public)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var category = await _service.GetByIdAsync(id);
        if (category == null) return NotFound("Category not found");
        return Ok(category);
    }

    // CREATE (Admin)
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
        var created = await _service.CreateAsync(category);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // UPDATE (Admin)
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Category category)
    {
        category.Id = id;
        var success = await _service.UpdateAsync(category);
        if (!success) return NotFound("Category not found");
        return Ok(category);
    }

    // DELETE (Admin)
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound("Category not found");
        return Ok(new { message = "Category deleted successfully" });
    }
}

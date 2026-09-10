using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;
using MongoDB.Bson;

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

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _service.GetAllAsync();
        return Ok(categories);
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return BadRequest("Invalid category id.");

        var category = await _service.GetByIdAsync(id);
        if (category == null)
            return NotFound("Category not found.");

        return Ok(category);
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            return BadRequest("Category name is required.");

        var created = await _service.CreateAsync(category.Name);
        if (created == null)
            return BadRequest("Invalid category name.");

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Category category)
    {
        if (!ObjectId.TryParse(id, out _))
            return BadRequest("Invalid category id.");

        if (string.IsNullOrWhiteSpace(category.Name))
            return BadRequest("Category name is required.");

        var updated = await _service.UpdateAsync(id, category.Name);
        if (!updated)
            return NotFound("Category not found.");

        return NoContent();
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return BadRequest("Invalid category id.");

        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound("Category not found.");

        return NoContent();
    }
}

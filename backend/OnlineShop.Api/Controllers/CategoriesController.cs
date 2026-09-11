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

    private bool IsValidObjectId(string? id)
    {
        return !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
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
    public async Task<IActionResult> GetById(string? id)
    {
        if (!IsValidObjectId(id))
            return BadRequest("Invalid category id.");

        var category = await _service.GetByIdAsync(id!);
        if (category == null)
            return NotFound("Category not found.");

        return Ok(category);
    }

    // ---------------------------------------------------------
    // CREATE (Admin)
    // ---------------------------------------------------------
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Category? category)
    {
        if (!IsAdmin())
            return Unauthorized("Admin only.");

        if (category == null)
            return BadRequest("Invalid category data.");

        var name = category.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Category name is required.");

        var created = await _service.CreateAsync(name);
        if (created == null)
            return BadRequest("Invalid category name.");

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // ---------------------------------------------------------
    // UPDATE (Admin)
    // ---------------------------------------------------------
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string? id, Category? category)
    {
        if (!IsAdmin())
            return Unauthorized("Admin only.");

        if (!IsValidObjectId(id))
            return BadRequest("Invalid category id.");

        if (category == null)
            return BadRequest("Invalid category data.");

        var name = category.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Category name is required.");

        var updated = await _service.UpdateAsync(id!, name);

        if (!updated)
            return NotFound("Category not found.");

        return NoContent();
    }

    // ---------------------------------------------------------
    // DELETE (Admin)
    // ---------------------------------------------------------
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string? id)
    {
        if (!IsAdmin())
            return Unauthorized("Admin only.");

        if (!IsValidObjectId(id))
            return BadRequest("Invalid category id.");

        var deleted = await _service.DeleteAsync(id!);

        if (!deleted)
            return NotFound("Category not found.");

        return NoContent();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;
using MongoDB.Bson;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
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
    // GET ALL PRODUCTS (Public)
    // ---------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _service.GetAllAsync();
        return Ok(products);
    }

    // ---------------------------------------------------------
    // GET PRODUCT BY ID (Public)
    // ---------------------------------------------------------
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string? id)
    {
        if (!IsValidObjectId(id))
            return BadRequest("Invalid product id.");

        var product = await _service.GetByIdAsync(id!);
        if (product == null)
            return NotFound("Product not found.");

        return Ok(product);
    }

    // ---------------------------------------------------------
    // SEARCH + FILTERING (Public)
    // ---------------------------------------------------------
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        string? keyword,
        string? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        bool? inStock,
        string? sortBy,
        bool descending = false
    )
    {
        if (categoryId != null && !IsValidObjectId(categoryId))
            return BadRequest("Invalid category id.");

        if (minPrice < 0 || maxPrice < 0)
            return BadRequest("Price cannot be negative.");

        var results = await _service.SearchAsync(
            keyword,
            categoryId,
            minPrice,
            maxPrice,
            inStock,
            sortBy,
            descending
        );

        return Ok(results);
    }

    // ---------------------------------------------------------
    // CREATE PRODUCT (Admin only)
    // ---------------------------------------------------------
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Product? product)
    {
        if (!IsAdmin())
            return Unauthorized("Admin only.");

        if (product == null)
            return BadRequest("Invalid product data.");

        var name = product.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Product name is required.");

        if (product.Price <= 0)
            return BadRequest("Price must be greater than zero.");

        product.Name = name;

        var created = await _service.CreateAsync(product);

        if (created == null)
            return BadRequest("Invalid product data.");

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // ---------------------------------------------------------
    // UPDATE PRODUCT (Admin only)
    // ---------------------------------------------------------
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string? id, Product? product)
    {
        if (!IsAdmin())
            return Unauthorized("Admin only.");

        if (!IsValidObjectId(id))
            return BadRequest("Invalid product id.");

        if (product == null)
            return BadRequest("Invalid product data.");

        var name = product.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Product name is required.");

        if (product.Price <= 0)
            return BadRequest("Price must be greater than zero.");

        product.Id = id!;
        product.Name = name;

        var success = await _service.UpdateAsync(product);

        if (!success)
            return NotFound("Product not found.");

        return Ok(product);
    }

    // ---------------------------------------------------------
    // DELETE PRODUCT (Admin only)
    // ---------------------------------------------------------
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string? id)
    {
        if (!IsAdmin())
            return Unauthorized("Admin only.");

        if (!IsValidObjectId(id))
            return BadRequest("Invalid product id.");

        var success = await _service.DeleteAsync(id!);

        if (!success)
            return NotFound("Product not found.");

        return Ok(new { message = "Product deleted successfully" });
    }
}

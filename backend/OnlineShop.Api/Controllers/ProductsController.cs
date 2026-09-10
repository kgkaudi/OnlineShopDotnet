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
    public async Task<IActionResult> GetById(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return BadRequest("Invalid product id.");

        var product = await _service.GetByIdAsync(id);
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
        if (categoryId != null && !ObjectId.TryParse(categoryId, out _))
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
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            return BadRequest("Product name is required.");

        if (product.Price <= 0)
            return BadRequest("Price must be greater than zero.");

        var created = await _service.CreateAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // ---------------------------------------------------------
    // UPDATE PRODUCT (Admin only)
    // ---------------------------------------------------------
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Product product)
    {
        if (!ObjectId.TryParse(id, out _))
            return BadRequest("Invalid product id.");

        if (string.IsNullOrWhiteSpace(product.Name))
            return BadRequest("Product name is required.");

        if (product.Price <= 0)
            return BadRequest("Price must be greater than zero.");

        product.Id = id;

        var success = await _service.UpdateAsync(product);
        if (!success)
            return NotFound("Product not found.");

        return Ok(product);
    }

    // ---------------------------------------------------------
    // DELETE PRODUCT (Admin only)
    // ---------------------------------------------------------
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return BadRequest("Invalid product id.");

        var success = await _service.DeleteAsync(id);
        if (!success)
            return NotFound("Product not found.");

        return Ok(new { message = "Product deleted successfully" });
    }
}

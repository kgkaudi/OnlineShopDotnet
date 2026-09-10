using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Services;
using MongoDB.Bson;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    private string? GetUserId() => User.FindFirst("sub")?.Value;

    // ---------------------------------------------------------
    // GET ALL USERS (Admin only)
    // ---------------------------------------------------------
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _service.GetAllAsync();
        return Ok(users);
    }

    // ---------------------------------------------------------
    // GET USER BY ID (Admin or the user themselves)
    // ---------------------------------------------------------
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var currentUserId = GetUserId();
        if (currentUserId == null || !ObjectId.TryParse(currentUserId, out _))
            return Unauthorized("Invalid user token.");

        if (!ObjectId.TryParse(id, out _))
            return BadRequest("Invalid user id.");

        var isAdmin = User.IsInRole("Admin");

        var user = await _service.GetByIdAsync(id, currentUserId, isAdmin);

        if (user == null)
            return Forbid();

        return Ok(user);
    }

    // ---------------------------------------------------------
    // ADD ROLE TO USER (Admin only)
    // ---------------------------------------------------------
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/roles")]
    public async Task<IActionResult> AddRole(string id, [FromBody] string role)
    {
        if (!ObjectId.TryParse(id, out _))
            return BadRequest("Invalid user id.");

        if (string.IsNullOrWhiteSpace(role))
            return BadRequest("Role is required.");

        var success = await _service.AddRoleAsync(id, role);
        if (!success)
            return NotFound("User not found.");

        return Ok(new { message = $"Role '{role}' added to user {id}" });
    }

    // ---------------------------------------------------------
    // DELETE USER (Admin only)
    // ---------------------------------------------------------
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return BadRequest("Invalid user id.");

        var success = await _service.DeleteAsync(id);
        if (!success)
            return NotFound("User not found.");

        return Ok(new { message = "User deleted successfully" });
    }
}

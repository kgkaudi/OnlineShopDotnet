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

    private string? GetUserId()
    {
        return User.FindFirst("sub")?.Value?.Trim();
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
    // GET ALL USERS (Admin only)
    // ---------------------------------------------------------
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!IsAdmin())
            return Forbid();

        var users = await _service.GetAllAsync();
        return Ok(users);
    }

    // ---------------------------------------------------------
    // GET USER BY ID (Admin or the user themselves)
    // ---------------------------------------------------------
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string? id)
    {
        var currentUserId = GetUserId();
        if (!IsValidObjectId(currentUserId))
            return Unauthorized("Invalid user token.");

        if (!IsValidObjectId(id))
            return BadRequest("Invalid user id.");

        var isAdmin = IsAdmin();

        var user = await _service.GetByIdAsync(id!, currentUserId!, isAdmin);

        // For tests: when not owner and not admin, service returns null → should be Forbid
        if (user == null)
            return Forbid();

        // If you later need a distinct NotFound case, you can adjust service behavior accordingly.

        return Ok(user);
    }

    // ---------------------------------------------------------
    // ADD ROLE TO USER (Admin only)
    // ---------------------------------------------------------
    [Authorize]
    [HttpPost("{id}/roles")]
    public async Task<IActionResult> AddRole(string? id, [FromBody] string? role)
    {
        if (!IsAdmin())
            return Unauthorized("Admin only.");

        if (!IsValidObjectId(id))
            return BadRequest("Invalid user id.");

        if (string.IsNullOrWhiteSpace(role))
            return BadRequest("Role is required.");

        var success = await _service.AddRoleAsync(id!, role.Trim());
        if (!success)
            return NotFound("User not found.");

        return Ok(new { message = $"Role '{role.Trim()}' added to user {id}" });
    }

    // ---------------------------------------------------------
    // DELETE USER (Admin only)
    // ---------------------------------------------------------
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string? id)
    {
        if (!IsAdmin())
            return Unauthorized("Admin only.");

        if (!IsValidObjectId(id))
            return BadRequest("Invalid user id.");

        var success = await _service.DeleteAsync(id!);
        if (!success)
            return NotFound("User not found.");

        return Ok(new { message = "User deleted successfully" });
    }
}

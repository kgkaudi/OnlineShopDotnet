using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Services;
using MongoDB.Bson;
using OnlineShop.Api.DTOs;

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

        // Fetch with unrestricted visibility (bypass the ownership check inside
        // the service) so we can tell "doesn't exist" (NotFound) apart from
        // "exists but isn't yours" (Forbid) — the service itself returns null
        // for both cases when queried with the real caller's identity.
        var existenceCheck = await _service.GetByIdAsync(id!, id!, true);
        if (existenceCheck == null)
            return NotFound("User not found.");

        if (!isAdmin && currentUserId != id)
            return Forbid();

        return Ok(existenceCheck);
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
    // UPDATE USER PROFILE (Admin or the user themselves)
    // ---------------------------------------------------------
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string? id, [FromBody] UpdateProfileDto? dto)
    {
        var currentUserId = GetUserId();

        if (!IsValidObjectId(currentUserId))
            return Unauthorized("Invalid user token.");

        if (!IsValidObjectId(id))
            return BadRequest("Invalid user id.");

        if (dto == null)
            return BadRequest("Invalid request.");

        var isAdmin = IsAdmin();

        // Normal users can only update their own profile.
        if (!isAdmin && currentUserId != id)
            return Forbid();

        if (string.IsNullOrWhiteSpace(dto.FullName))
            return BadRequest("Full name is required.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Email is required.");

        if (!dto.Email.Contains("@"))
            return BadRequest("Invalid email format.");

        var updatedUser = await _service.UpdateProfileAsync(
            id!,
            dto.FullName.Trim(),
            dto.Email.Trim()
        );

        if (updatedUser == null)
            return NotFound("User not found.");

        return Ok(new
        {
            id = updatedUser.Id,
            email = updatedUser.Email,
            fullName = updatedUser.FullName,
            roles = updatedUser.Roles
        });
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

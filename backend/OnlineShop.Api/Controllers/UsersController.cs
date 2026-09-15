using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Services;
using MongoDB.Bson;
using OnlineShop.Api.DTOs;
using OnlineShop.Api.Models;

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
        return User.FindFirst("sub")?.Value?.Trim()
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?.Value?.Trim();
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

        return Ok(users.Select(MapUserResponse));
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

        // Fetch without ownership restriction so we can distinguish
        // between NotFound and Forbid.
        var existenceCheck = await _service.GetByIdAsync(
            id!,
            id!,
            true
        );

        if (existenceCheck == null)
            return NotFound("User not found.");

        if (!isAdmin && currentUserId != id)
            return Forbid();

        return Ok(MapUserResponse(existenceCheck));
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

        role = role.Trim();

        var success = await _service.AddRoleAsync(id!, role);

        if (!success)
            return NotFound("User not found.");

        return Ok(new { message = $"Role '{role}' added to user {id}" });
    }

    // ---------------------------------------------------------
    // UPDATE USER PROFILE
    // Admin or the user themselves
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

        // -----------------------------------------------------
        // Validate phone number if supplied
        // -----------------------------------------------------

        if (dto.PhoneNumber != null &&
            string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            dto.PhoneNumber = null;
        }

        // -----------------------------------------------------
        // UPDATE
        // -----------------------------------------------------

        var updatedUser = await _service.UpdateProfileAsync(
            id!,
            dto.FullName.Trim(),
            dto.Email.Trim(),
            dto.PhoneNumber,
            dto.ShippingAddress,
            dto.BillingAddress
        );

        if (updatedUser == null)
            return NotFound("User not found or email already exists.");

        return Ok(MapUserResponse(updatedUser));
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

    // ---------------------------------------------------------
    // USER RESPONSE MAPPING
    // ---------------------------------------------------------
    private static object MapUserResponse(User user)
    {
        return new
        {
            id = user.Id,
            email = user.Email,
            fullName = user.FullName,
            roles = user.Roles,
            phoneNumber = user.PhoneNumber,
            shippingAddress = user.ShippingAddress,
            billingAddress = user.BillingAddress,
            createdAt = user.CreatedAt,
            updatedAt = user.UpdatedAt,
            isEmailVerified = user.IsEmailVerified
        };
    }
}
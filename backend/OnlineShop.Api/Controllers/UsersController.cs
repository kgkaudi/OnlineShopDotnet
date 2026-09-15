using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using OnlineShop.Api.DTOs;
using OnlineShop.Api.Models;
using OnlineShop.Api.Services;
using System.Security.Claims;

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

    // ---------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------

    private string? GetUserId()
    {
        return User.FindFirst("sub")?.Value?.Trim()
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value?.Trim();
    }

    private static bool IsValidObjectId(string? id)
    {
        return !string.IsNullOrWhiteSpace(id)
            && ObjectId.TryParse(id, out _);
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    // ---------------------------------------------------------
    // GET ALL USERS
    // Admin only
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
    // GET USER BY ID
    // Admin or the user themselves
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

        // First determine whether the requested user exists.
        var user = await _service.GetByIdAsync(
            id!,
            id!,
            true
        );

        if (user == null)
            return NotFound("User not found.");

        // Normal users can only access their own profile.
        if (!isAdmin && currentUserId != id)
            return Forbid();

        return Ok(MapUserResponse(user));
    }

    // ---------------------------------------------------------
    // ADD ROLE
    // Admin only
    // ---------------------------------------------------------

    [Authorize]
    [HttpPost("{id}/roles")]
    public async Task<IActionResult> AddRole(
        string? id,
        [FromBody] string? role)
    {
        if (!IsAdmin())
            return Forbid();

        if (!IsValidObjectId(id))
            return BadRequest("Invalid user id.");

        if (string.IsNullOrWhiteSpace(role))
            return BadRequest("Role is required.");

        role = role.Trim();

        var success = await _service.AddRoleAsync(
            id!,
            role
        );

        if (!success)
            return NotFound("User not found.");

        return Ok(new
        {
            message = $"Role '{role}' added to user {id}"
        });
    }

    // ---------------------------------------------------------
    // UPDATE ROLES
    // Admin only
    //
    // Body:
    // {
    //     "roles": ["User", "Admin"]
    // }
    // ---------------------------------------------------------

    [Authorize]
    [HttpPut("{id}/roles")]
    public async Task<IActionResult> UpdateRoles(
        string? id,
        [FromBody] UpdateRolesDto? dto)
    {
        if (!IsAdmin())
            return Forbid();

        if (!IsValidObjectId(id))
            return BadRequest("Invalid user id.");

        if (dto == null)
            return BadRequest("Invalid request.");

        if (dto.Roles == null || dto.Roles.Count == 0)
            return BadRequest("At least one role is required.");

        var requestedRoles = dto.Roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (requestedRoles.Count == 0)
            return BadRequest("At least one valid role is required.");

        // Only roles supported by the application are allowed.
        var allowedRoles = new HashSet<string>(
            new[] { "User", "Admin" },
            StringComparer.OrdinalIgnoreCase);

        if (requestedRoles.Any(role => !allowedRoles.Contains(role)))
        {
            return BadRequest(
                "Invalid role. Allowed roles are: User, Admin.");
        }

        // Prevent an administrator from removing their own Admin role.
        var currentUserId = GetUserId();

        if (IsValidObjectId(currentUserId) &&
            string.Equals(currentUserId, id, StringComparison.OrdinalIgnoreCase) &&
            !requestedRoles.Any(role =>
                string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(
                "You cannot remove the Admin role from your own account.");
        }

        var updatedUser = await _service.UpdateRolesAsync(
            id!,
            requestedRoles);

        if (updatedUser == null)
            return NotFound("User not found.");

        return Ok(MapUserResponse(updatedUser));
    }

    // ---------------------------------------------------------
    // UPDATE USER PROFILE
    // Admin or the user themselves
    // ---------------------------------------------------------

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string? id,
        [FromBody] UpdateProfileDto? dto)
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

        // Basic email validation.
        if (!dto.Email.Contains("@"))
            return BadRequest("Invalid email format.");

        // Empty phone number is treated as null.
        if (dto.PhoneNumber != null &&
            string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            dto.PhoneNumber = null;
        }

        var updatedUser = await _service.UpdateProfileAsync(
            id!,
            dto.FullName.Trim(),
            dto.Email.Trim(),
            dto.PhoneNumber,
            dto.ShippingAddress,
            dto.BillingAddress
        );

        if (updatedUser == null)
            return NotFound(
                "User not found or email already exists.");

        return Ok(MapUserResponse(updatedUser));
    }

    // ---------------------------------------------------------
    // DELETE USER
    // Admin only
    //
    // UserService also deletes:
    // - user's cart
    // - user's wishlist
    // ---------------------------------------------------------

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string? id)
    {
        if (!IsAdmin())
            return Forbid();

        if (!IsValidObjectId(id))
            return BadRequest("Invalid user id.");

        // Prevent an administrator from deleting their own account.
        var currentUserId = GetUserId();

        if (IsValidObjectId(currentUserId) &&
            string.Equals(currentUserId, id, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("You cannot delete your own account.");
        }

        var success = await _service.DeleteAsync(id!);

        if (!success)
            return NotFound("User not found.");

        return Ok(new
        {
            message = "User, cart and wishlist deleted successfully."
        });
    }

    // ---------------------------------------------------------
    // USER RESPONSE MAPPING
    //
    // IMPORTANT:
    // PasswordHash is intentionally NOT returned.
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
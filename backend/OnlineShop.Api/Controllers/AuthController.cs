using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Dtos;
using OnlineShop.Api.Services;
using System.Security.Claims;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IJwtService _jwt;

    public AuthController(
        IAuthService auth,
        IJwtService jwt)
    {
        _auth = auth;
        _jwt = jwt;
    }

    // ---------------------------------------------------------
    // REGISTER
    // ---------------------------------------------------------

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto? dto)
    {
        if (dto == null)
            return BadRequest("Invalid request.");

        var email = dto.Email?.Trim();
        var password = dto.Password?.Trim();
        var fullName = dto.FullName?.Trim();

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required.");

        if (string.IsNullOrWhiteSpace(password))
            return BadRequest("Password is required.");

        if (string.IsNullOrWhiteSpace(fullName))
            return BadRequest("Full name is required.");

        if (!email.Contains("@"))
            return BadRequest("Invalid email format.");

        if (password.Length < 3)
            return BadRequest("Password is too short.");

        var phoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber)
            ? null
            : dto.PhoneNumber.Trim();

        var user = await _auth.RegisterAsync(
            email,
            password,
            fullName,
            phoneNumber,
            dto.ShippingAddress,
            dto.BillingAddress);

        if (user == null)
            return Conflict("Email already exists.");

        return Ok(new
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
        });
    }

    // ---------------------------------------------------------
    // LOGIN
    // ---------------------------------------------------------

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto? dto)
    {
        if (dto == null)
            return BadRequest("Invalid request.");

        var email = dto.Email?.Trim();
        var password = dto.Password?.Trim();

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required.");

        if (string.IsNullOrWhiteSpace(password))
            return BadRequest("Password is required.");

        if (!email.Contains("@"))
            return BadRequest("Invalid email format.");

        var token = await _auth.LoginAsync(
            email,
            password);

        if (token == null)
            return Unauthorized("Invalid credentials.");

        return Ok(new
        {
            token,
            expires = _jwt.GetExpiration()
        });
    }

    // ---------------------------------------------------------
    // LOGOUT
    // Real token invalidation
    // ---------------------------------------------------------

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // -----------------------------------------------------
        // Extract user ID from claims
        //
        // JWT normally uses "sub".
        // NameIdentifier is supported for ASP.NET compatibility.
        // "id" and "userId" are supported for older tokens/tests.
        // -----------------------------------------------------

        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("Invalid token.");

        // -----------------------------------------------------
        // Extract Bearer token
        //
        // Supports:
        //   Bearer token
        //   Bearer    token
        //   bearer token
        // -----------------------------------------------------

        var token = ExtractBearerToken();

        if (string.IsNullOrWhiteSpace(token))
            return Unauthorized("Invalid authorization header.");

        // -----------------------------------------------------
        // Invalidate token
        // -----------------------------------------------------

        var success = await _auth.LogoutAsync(
            userId,
            token);

        if (!success)
            return BadRequest("Logout failed.");

        return Ok(new
        {
            message = "Logged out successfully."
        });
    }

    // ---------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------

    private string? GetUserId()
    {
        var userId =
            User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("id")?.Value
            ?? User.FindFirst("userId")?.Value;

        return userId?.Trim();
    }

    private string? ExtractBearerToken()
    {
        var authorization =
            Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorization))
            return null;

        var parts = authorization
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            return null;

        if (!parts[0].Equals(
                "Bearer",
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var token = parts[1].Trim();

        return string.IsNullOrWhiteSpace(token)
            ? null
            : token;
    }
}
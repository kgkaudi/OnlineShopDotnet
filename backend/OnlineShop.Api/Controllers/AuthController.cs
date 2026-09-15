using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Dtos;
using OnlineShop.Api.Services;

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
    public async Task<IActionResult> Register(
        RegisterDto? dto)
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

        var phoneNumber = string.IsNullOrWhiteSpace(
            dto.PhoneNumber)
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
    public async Task<IActionResult> Login(
        LoginDto? dto)
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
    // LOGOUT (REAL TOKEN INVALIDATION)
    // ---------------------------------------------------------

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId =
            User.FindFirst("id")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Invalid token.");

        // Extract token from Authorization header
        var authHeader =
            Request.Headers["Authorization"].ToString();

        var token = authHeader
            .Replace("Bearer ", "")
            .Trim();

        if (string.IsNullOrWhiteSpace(token))
            return Unauthorized("Missing token.");

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
}
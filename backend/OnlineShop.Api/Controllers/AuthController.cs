using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Services;
using OnlineShop.Api.Dtos;
using MongoDB.Bson;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly IJwtService _jwt;

    public AuthController(AuthService auth, IJwtService jwt)
    {
        _auth = auth;
        _jwt = jwt;
    }

    // ---------------------------------------------------------
    // REGISTER
    // ---------------------------------------------------------

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        // Basic DTO validation
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Email is required.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Password is required.");

        if (string.IsNullOrWhiteSpace(dto.FullName))
            return BadRequest("Full name is required.");

        // Email format validation
        if (!dto.Email.Contains("@"))
            return BadRequest("Invalid email format.");

        // Attempt registration
        var user = await _auth.RegisterAsync(dto.Email, dto.Password, dto.FullName);

        if (user == null)
            return Conflict("Email already exists.");

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            fullName = user.FullName,
            roles = user.Roles
        });
    }

    // ---------------------------------------------------------
    // LOGIN
    // ---------------------------------------------------------

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        // Basic DTO validation
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Email is required.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Password is required.");

        // Email format validation
        if (!dto.Email.Contains("@"))
            return BadRequest("Invalid email format.");

        // Attempt login
        var token = await _auth.LoginAsync(dto.Email, dto.Password);

        if (token == null)
            return Unauthorized("Invalid credentials.");

        return Ok(new
        {
            token,
            expires = _jwt.GetExpiration()
        });
    }
}

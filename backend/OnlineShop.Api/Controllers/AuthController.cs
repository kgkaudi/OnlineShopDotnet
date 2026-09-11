using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Services;
using OnlineShop.Api.Dtos;

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
    public async Task<IActionResult> Register(RegisterDto? dto)
    {
        // Null DTO
        if (dto == null)
            return BadRequest("Invalid request.");

        var email = dto.Email?.Trim();
        var password = dto.Password?.Trim();
        var fullName = dto.FullName?.Trim();

        // Basic validation
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required.");

        if (string.IsNullOrWhiteSpace(password))
            return BadRequest("Password is required.");

        if (string.IsNullOrWhiteSpace(fullName))
            return BadRequest("Full name is required.");

        // Email format validation
        if (!email.Contains("@"))
            return BadRequest("Invalid email format.");

        // Password length validation (tests expect rejection)
        if (password.Length < 3)
            return BadRequest("Password is too short.");

        // Attempt registration
        var user = await _auth.RegisterAsync(email, password, fullName);

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
    public async Task<IActionResult> Login(LoginDto? dto)
    {
        // Null DTO
        if (dto == null)
            return BadRequest("Invalid request.");

        var email = dto.Email?.Trim();
        var password = dto.Password?.Trim();

        // Basic validation
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required.");

        if (string.IsNullOrWhiteSpace(password))
            return BadRequest("Password is required.");

        // Email format validation
        if (!email.Contains("@"))
            return BadRequest("Invalid email format.");

        // Attempt login
        var token = await _auth.LoginAsync(email, password);

        if (token == null)
            return Unauthorized("Invalid credentials.");

        return Ok(new
        {
            token,
            expires = _jwt.GetExpiration()
        });
    }
}

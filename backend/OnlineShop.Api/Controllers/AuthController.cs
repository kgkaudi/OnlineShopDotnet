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

    // -----------------------------
    // Register
    // -----------------------------
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = await _auth.RegisterAsync(dto.Email, dto.Password, dto.FullName);

        if (user == null)
            return BadRequest("Email already exists");

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            fullName = user.FullName,
            roles = user.Roles
        });
    }

    // -----------------------------
    // Login
    // -----------------------------
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var token = await _auth.LoginAsync(dto.Email, dto.Password);

        if (token == null)
            return Unauthorized("Invalid credentials");

        return Ok(new
        {
            token,
            expires = _jwt.GetExpiration()
        });
    }
}

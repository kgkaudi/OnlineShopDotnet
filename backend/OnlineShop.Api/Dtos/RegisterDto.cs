using OnlineShop.Api.Models;

namespace OnlineShop.Api.Dtos;

public class RegisterDto
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public Address? ShippingAddress { get; set; }

    public Address? BillingAddress { get; set; }
}
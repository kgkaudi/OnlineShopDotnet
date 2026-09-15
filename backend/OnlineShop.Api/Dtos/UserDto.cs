using OnlineShop.Api.Models;

namespace OnlineShop.Api.DTOs;

public class UpdateProfileDto
{
    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public Address? ShippingAddress { get; set; }

    public Address? BillingAddress { get; set; }
}

public class UpdateRolesDto
{
    public List<string> Roles { get; set; } = new();
}
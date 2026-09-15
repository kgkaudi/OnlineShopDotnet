using OnlineShop.Api.Models;

namespace OnlineShop.Api.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(
        string email,
        string password,
        string fullName,
        string? phoneNumber,
        Address? shippingAddress,
        Address? billingAddress);

    Task<string?> LoginAsync(string email, string password);
    Task<bool> AddRoleAsync(string userId, string role);
    Task<bool> LogoutAsync(string userId, string token);
}
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Services;

public interface IJwtService
{
    string GenerateToken(User user);
    DateTime GetExpiration();
}

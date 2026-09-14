using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public interface IInvalidTokenRepository
{
    Task AddAsync(string token);
    Task<bool> ExistsAsync(string token);
}

using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public interface ICouponRepository
{
    Task<Coupon?> GetByCodeAsync(string code);
    Task<List<Coupon>> GetAllAsync();
    Task CreateAsync(Coupon coupon);
    Task<bool> UpdateAsync(Coupon coupon);
    Task<bool> DeleteAsync(string id);
}

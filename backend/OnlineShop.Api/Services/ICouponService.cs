using OnlineShop.Api.Models;

namespace OnlineShop.Api.Services;

public interface ICouponService
{
    Task<Coupon?> ValidateAsync(string code);
    Task<List<Coupon>> GetAllAsync();
    Task<List<Coupon>> GetMineAsync(string userId);
    Task<Coupon?> CreateAsync(Coupon coupon);
    Task<bool> DeleteAsync(string id);
}

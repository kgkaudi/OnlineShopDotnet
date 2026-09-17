using OnlineShop.Api.Models;

public interface ICouponRepository
{
    Task<Coupon?> GetByCodeAsync(string code);
    Task<Coupon?> GetByIdAsync(string id);
    Task<List<Coupon>> GetAllAsync();
    Task CreateAsync(Coupon coupon);
    Task<bool> UpdateAsync(Coupon coupon);
    Task<bool> IncrementUsageAsync(string id);
    Task<bool> DeleteAsync(string id);
}

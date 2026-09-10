using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;

namespace OnlineShop.Api.Services;

public class CouponService : ICouponService
{
    private readonly ICouponRepository _repo;

    public CouponService(ICouponRepository repo)
    {
        _repo = repo;
    }

    public async Task<Coupon?> ValidateAsync(string code)
    {
        var coupon = await _repo.GetByCodeAsync(code);
        if (coupon == null) return null;

        if (!coupon.Active) return null;
        if (coupon.Expiration < DateTime.UtcNow) return null;
        if (coupon.UsedCount >= coupon.MaxUsage) return null;

        return coupon;
    }

    public async Task<List<Coupon>> GetAllAsync() =>
        await _repo.GetAllAsync();

    public async Task<Coupon> CreateAsync(Coupon coupon)
    {
        await _repo.CreateAsync(coupon);
        return coupon;
    }

    public async Task<bool> DeleteAsync(string id) =>
        await _repo.DeleteAsync(id);
}

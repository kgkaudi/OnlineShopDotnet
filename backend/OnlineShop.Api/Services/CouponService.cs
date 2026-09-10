using MongoDB.Bson;
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

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    public async Task<List<Coupon>> GetAllAsync() =>
        await _repo.GetAllAsync();

    // ---------------------------------------------------------
    // GET BY CODE
    // ---------------------------------------------------------

    public async Task<Coupon?> GetByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        return await _repo.GetByCodeAsync(code);
    }

    // ---------------------------------------------------------
    // VALIDATE
    // ---------------------------------------------------------

    public async Task<Coupon?> ValidateAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var coupon = await _repo.GetByCodeAsync(code);
        if (coupon == null)
            return null;

        if (!coupon.Active)
            return null;

        if (coupon.Expiration < DateTime.UtcNow)
            return null;

        if (coupon.UsedCount >= coupon.MaxUsage)
            return null;

        return coupon;
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task<Coupon?> CreateAsync(Coupon coupon)
    {
        if (string.IsNullOrWhiteSpace(coupon.Code))
            return null;

        coupon.Id = ObjectId.GenerateNewId().ToString();

        await _repo.CreateAsync(coupon);
        return coupon;
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    public async Task<bool> DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        return await _repo.DeleteAsync(id);
    }
}

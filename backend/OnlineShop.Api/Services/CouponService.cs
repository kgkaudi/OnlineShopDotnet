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
    // VALIDATION HELPERS
    // ---------------------------------------------------------

    private static string? NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        return code.Trim();
    }

    private static bool IsValidObjectId(string id)
    {
        return !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);
    }

    private static bool IsValidCoupon(Coupon coupon)
    {
        if (coupon == null)
            return false;

        if (NormalizeCode(coupon.Code) == null)
            return false;

        if (string.IsNullOrWhiteSpace(coupon.Type))
            return false;

        if (coupon.Value <= 0)
            return false;

        if (coupon.MaxUsage <= 0)
            return false;

        if (coupon.Expiration == default)
            return false;

        if (coupon.UsedCount < 0)
            return false;

        return true;
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
        var normalized = NormalizeCode(code);
        if (normalized == null)
            return null;

        return await _repo.GetByCodeAsync(normalized);
    }

    // ---------------------------------------------------------
    // VALIDATE
    // ---------------------------------------------------------

    public async Task<Coupon?> ValidateAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var coupon = await _repo.GetByCodeAsync(code.Trim());
        if (coupon == null)
            return null;

        if (!coupon.Active)
            return null;

        if (coupon.Expiration <= DateTime.UtcNow)
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
        if (coupon == null)
            return null;

        if (string.IsNullOrWhiteSpace(coupon.Code))
            return null;

        if (string.IsNullOrWhiteSpace(coupon.Type))
            coupon.Type = "Default";

        if (coupon.Value <= 0)
            return null;

        if (coupon.MaxUsage <= 0)
            return null;

        if (string.IsNullOrWhiteSpace(coupon.Id) || !ObjectId.TryParse(coupon.Id, out _))
            coupon.Id = ObjectId.GenerateNewId().ToString();

        // ensure UsedCount starts at 0
        coupon.UsedCount = 0;

        await _repo.CreateAsync(coupon);
        return coupon;
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    public async Task<bool> DeleteAsync(string id)
    {
        if (!IsValidObjectId(id))
            return false;

        return await _repo.DeleteAsync(id);
    }
}

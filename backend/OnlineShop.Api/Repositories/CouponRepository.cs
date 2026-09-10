using MongoDB.Driver;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly IMongoCollection<Coupon> _coupons;

    public CouponRepository(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionURI"]);
        var db = client.GetDatabase(config["MongoDB:DatabaseName"]);
        _coupons = db.GetCollection<Coupon>("Coupons");
    }

    public async Task<Coupon?> GetByCodeAsync(string code) =>
        await _coupons.Find(c => c.Code == code).FirstOrDefaultAsync();

    public async Task<List<Coupon>> GetAllAsync() =>
        await _coupons.Find(_ => true).ToListAsync();

    public async Task CreateAsync(Coupon coupon) =>
        await _coupons.InsertOneAsync(coupon);

    public async Task<bool> UpdateAsync(Coupon coupon)
    {
        var result = await _coupons.ReplaceOneAsync(c => c.Id == coupon.Id, coupon);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _coupons.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}

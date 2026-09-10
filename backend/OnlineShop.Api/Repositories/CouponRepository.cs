using MongoDB.Driver;
using MongoDB.Bson;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly IMongoCollection<Coupon> _coupons;

    public CouponRepository(IConfiguration config)
    {
        var connectionUri = config["MongoDB:ConnectionURI"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionURI missing in configuration.");

        var dbName = config["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDB:DatabaseName missing in configuration.");

        var client = new MongoClient(connectionUri);
        var db = client.GetDatabase(dbName);

        _coupons = db.GetCollection<Coupon>("Coupons");
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task CreateAsync(Coupon coupon)
    {
        if (string.IsNullOrWhiteSpace(coupon.Id))
            coupon.Id = ObjectId.GenerateNewId().ToString();

        await _coupons.InsertOneAsync(coupon);
    }

    // ---------------------------------------------------------
    // READ
    // ---------------------------------------------------------

    public async Task<Coupon?> GetByCodeAsync(string code) =>
        await _coupons.Find(c => c.Code == code).FirstOrDefaultAsync();

    public async Task<List<Coupon>> GetAllAsync() =>
        await _coupons.Find(_ => true).ToListAsync();

    public async Task<Coupon?> GetByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _coupons.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    public async Task<bool> UpdateAsync(Coupon coupon)
    {
        if (!ObjectId.TryParse(coupon.Id, out _))
            return false;

        var result = await _coupons.ReplaceOneAsync(c => c.Id == coupon.Id, coupon);

        // Treat "no modification" as success if the coupon exists
        return result.MatchedCount > 0;
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    public async Task<bool> DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var result = await _coupons.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}

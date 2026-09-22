using MongoDB.Driver;
using MongoDB.Bson;
using OnlineShop.Api.Models;
using System.Text.RegularExpressions;

namespace OnlineShop.Api.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly IMongoCollection<Coupon> _coupons;

    public CouponRepository(IConfiguration config)
    {
        var connectionString = config["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionString missing.");

        var dbName = config["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDB:DatabaseName missing.");

        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(dbName);

        _coupons = db.GetCollection<Coupon>("Coupons");
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------
    public async Task CreateAsync(Coupon coupon)
    {
        if (coupon == null)
            throw new ArgumentNullException(nameof(coupon));

        if (string.IsNullOrWhiteSpace(coupon.Code))
            throw new ArgumentNullException(nameof(coupon.Code));

        coupon.Code = coupon.Code.Trim();

        if (string.IsNullOrWhiteSpace(coupon.Type))
            throw new ArgumentNullException(nameof(coupon.Type));

        if (coupon.Type != "percentage" && coupon.Type != "fixed")
            throw new ArgumentException("Invalid coupon type.", nameof(coupon.Type));

        if (coupon.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(coupon.Value));

        if (coupon.MaxUsage < 0)
            throw new ArgumentOutOfRangeException(nameof(coupon.MaxUsage));

        if (coupon.UsedCount < 0)
            throw new ArgumentOutOfRangeException(nameof(coupon.UsedCount));

        if (string.IsNullOrWhiteSpace(coupon.Id))
            coupon.Id = ObjectId.GenerateNewId().ToString();

        if (!ObjectId.TryParse(coupon.Id, out _))
            throw new ArgumentException("Invalid coupon ID format.", nameof(coupon.Id));

        await _coupons.InsertOneAsync(coupon);
    }

    // ---------------------------------------------------------
    // GET BY CODE
    // ---------------------------------------------------------
    public async Task<Coupon?> GetByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var normalized = code.Trim();

        var filter = Builders<Coupon>.Filter.Regex(
            c => c.Code,
            new BsonRegularExpression($"^{Regex.Escape(normalized)}$", "i")
        );

        return await _coupons.Find(filter).FirstOrDefaultAsync();
    }

    // ---------------------------------------------------------
    // GET ALL (Admin)
    // ---------------------------------------------------------
    public async Task<List<Coupon>> GetAllAsync() =>
        await _coupons.Find(_ => true).ToListAsync();

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------
    public async Task<Coupon?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _coupons.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    // ---------------------------------------------------------
    // UPDATE (Admin)
    // ---------------------------------------------------------
    public async Task<bool> UpdateAsync(Coupon coupon)
    {
        if (coupon == null)
            return false;

        if (string.IsNullOrWhiteSpace(coupon.Id))
            return false;

        if (!ObjectId.TryParse(coupon.Id, out _))
            return false;

        if (string.IsNullOrWhiteSpace(coupon.Code))
            return false;

        coupon.Code = coupon.Code.Trim();

        if (string.IsNullOrWhiteSpace(coupon.Type))
            return false;

        if (coupon.Value <= 0)
            return false;

        // UserId is optional — no validation needed

        var existing = await GetByIdAsync(coupon.Id);
        if (existing == null)
            return false;

        var result = await _coupons.ReplaceOneAsync(c => c.Id == coupon.Id, coupon);

        return result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool> IncrementUsageAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        if (!ObjectId.TryParse(id, out _))
            return false;

        var update = Builders<Coupon>.Update.Inc(c => c.UsedCount, 1);

        var result = await _coupons.UpdateOneAsync(
            c => c.Id == id,
            update
        );

        return result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    // ---------------------------------------------------------
    // DELETE (Admin)
    // ---------------------------------------------------------
    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        if (!ObjectId.TryParse(id, out _))
            return false;

        var result = await _coupons.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount == 1;
    }
}

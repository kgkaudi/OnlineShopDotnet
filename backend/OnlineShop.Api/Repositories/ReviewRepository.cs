using MongoDB.Driver;
using MongoDB.Bson;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly IMongoCollection<Review> _reviews;

    public ReviewRepository(IConfiguration config)
    {
        var connectionString = config["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionString missing in configuration.");

        var dbName = config["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDB:DatabaseName missing in configuration.");

        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(dbName);

        _reviews = db.GetCollection<Review>("Reviews");
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task CreateAsync(Review review)
    {
        if (string.IsNullOrWhiteSpace(review.Id))
            review.Id = ObjectId.GenerateNewId().ToString();

        await _reviews.InsertOneAsync(review);
    }

    // ---------------------------------------------------------
    // READ
    // ---------------------------------------------------------

    public async Task<List<Review>> GetByProductIdAsync(string productId)
    {
        if (!ObjectId.TryParse(productId, out _))
            return new List<Review>();

        return await _reviews.Find(r => r.ProductId == productId).ToListAsync();
    }

    public async Task<Review?> GetByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _reviews.Find(r => r.Id == id).FirstOrDefaultAsync();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    public async Task<bool> UpdateAsync(Review review)
    {
        if (!ObjectId.TryParse(review.Id, out _))
            return false;

        var result = await _reviews.ReplaceOneAsync(r => r.Id == review.Id, review);

        return result.MatchedCount > 0;
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    public async Task<bool> DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var result = await _reviews.DeleteOneAsync(r => r.Id == id);
        return result.DeletedCount > 0;
    }
}

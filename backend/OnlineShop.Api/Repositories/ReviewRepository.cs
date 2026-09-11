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
    // GET BY PRODUCT
    // ---------------------------------------------------------

    public async Task<List<Review>> GetByProductIdAsync(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            return new List<Review>();

        if (!ObjectId.TryParse(productId, out _))
            return new List<Review>();

        return await _reviews.Find(r => r.ProductId == productId).ToListAsync();
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    public async Task<Review?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _reviews.Find(r => r.Id == id).FirstOrDefaultAsync();
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task CreateAsync(Review review)
    {
        if (review == null)
            throw new ArgumentNullException(nameof(review));

        if (string.IsNullOrWhiteSpace(review.Id))
            review.Id = ObjectId.GenerateNewId().ToString();

        if (!ObjectId.TryParse(review.Id, out _))
            throw new ArgumentNullException(nameof(review.Id));

        if (string.IsNullOrWhiteSpace(review.ProductId))
            throw new ArgumentNullException(nameof(review.ProductId));

        if (!ObjectId.TryParse(review.ProductId, out _))
            throw new ArgumentNullException(nameof(review.ProductId));

        if (string.IsNullOrWhiteSpace(review.UserId))
            throw new ArgumentNullException(nameof(review.UserId));

        if (!ObjectId.TryParse(review.UserId, out _))
            throw new ArgumentNullException(nameof(review.UserId));

        if (review.Rating < 1 || review.Rating > 5)
            throw new ArgumentOutOfRangeException(nameof(review.Rating));

        await _reviews.InsertOneAsync(review);
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    public async Task<bool> UpdateAsync(Review review)
    {
        if (review == null)
            return false;

        if (string.IsNullOrWhiteSpace(review.Id))
            return false;

        if (!ObjectId.TryParse(review.Id, out _))
            return false;

        if (string.IsNullOrWhiteSpace(review.ProductId))
            return false;

        if (!ObjectId.TryParse(review.ProductId, out _))
            return false;

        if (string.IsNullOrWhiteSpace(review.UserId))
            return false;

        if (!ObjectId.TryParse(review.UserId, out _))
            return false;

        if (review.Rating < 1 || review.Rating > 5)
            return false;

        var existing = await GetByIdAsync(review.Id);
        if (existing == null)
            return false;

        var result = await _reviews.ReplaceOneAsync(r => r.Id == review.Id, review);

        return result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        if (!ObjectId.TryParse(id, out _))
            return false;

        var result = await _reviews.DeleteOneAsync(r => r.Id == id);

        return result.DeletedCount == 1;
    }
}

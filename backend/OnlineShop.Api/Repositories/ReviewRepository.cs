using MongoDB.Driver;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly IMongoCollection<Review> _reviews;

    public ReviewRepository(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionURI"]);
        var db = client.GetDatabase(config["MongoDB:DatabaseName"]);
        _reviews = db.GetCollection<Review>("Reviews");
    }

    public async Task<List<Review>> GetByProductIdAsync(string productId) =>
        await _reviews.Find(r => r.ProductId == productId).ToListAsync();

    public async Task<Review?> GetByIdAsync(string id) =>
        await _reviews.Find(r => r.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Review review) =>
        await _reviews.InsertOneAsync(review);

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _reviews.DeleteOneAsync(r => r.Id == id);
        return result.DeletedCount > 0;
    }
}

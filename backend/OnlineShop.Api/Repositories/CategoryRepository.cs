using MongoDB.Driver;
using MongoDB.Bson;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IMongoCollection<Category> _categories;

    public CategoryRepository(IConfiguration config)
    {
        var connectionString = config["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionString missing in configuration.");

        var dbName = config["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDB:DatabaseName missing in configuration.");

        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(dbName);

        _categories = db.GetCollection<Category>("Categories");
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task CreateAsync(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        if (string.IsNullOrWhiteSpace(category.Name))
            throw new ArgumentNullException(nameof(category.Name));

        if (string.IsNullOrWhiteSpace(category.Id))
            category.Id = ObjectId.GenerateNewId().ToString();

        await _categories.InsertOneAsync(category);
    }

    // ---------------------------------------------------------
    // READ
    // ---------------------------------------------------------

    public async Task<List<Category>> GetAllAsync() =>
        await _categories.Find(_ => true).ToListAsync();

    public async Task<Category?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _categories.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    public async Task<bool> UpdateAsync(Category category)
    {
        if (category == null)
            return false;

        if (string.IsNullOrWhiteSpace(category.Id))
            return false;

        if (!ObjectId.TryParse(category.Id, out _))
            return false;

        if (string.IsNullOrWhiteSpace(category.Name))
            return false;

        var existing = await GetByIdAsync(category.Id);
        if (existing == null)
            return false;

        var result = await _categories.ReplaceOneAsync(
            c => c.Id == category.Id,
            category
        );

        // Must be BOTH matched AND modified
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

        var result = await _categories.DeleteOneAsync(c => c.Id == id);

        return result.DeletedCount == 1;
    }
}

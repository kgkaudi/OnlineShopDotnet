using MongoDB.Bson;
using MongoDB.Driver;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IMongoCollection<Category> _categories;

    public CategoryRepository(IConfiguration config)
    {
        var connectionString = config["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException(
                "MongoDB:ConnectionString missing in configuration.");

        var dbName = config["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException(
                "MongoDB:DatabaseName missing in configuration.");

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(dbName);

        _categories = database.GetCollection<Category>("Category");
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
        {
            category.Id = ObjectId.GenerateNewId().ToString();
        }

        await _categories.InsertOneAsync(category);
    }

    // ---------------------------------------------------------
    // READ
    // ---------------------------------------------------------

    public async Task<List<Category>> GetAllAsync()
    {
        return await _categories
            .Find(FilterDefinition<Category>.Empty)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _categories
            .Find(category => category.Id == id)
            .FirstOrDefaultAsync();
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
            existingCategory =>
                existingCategory.Id == category.Id,
            category
        );

        return result.MatchedCount == 1 &&
               result.ModifiedCount == 1;
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

        var result = await _categories.DeleteOneAsync(
            category => category.Id == id
        );

        return result.DeletedCount == 1;
    }
}

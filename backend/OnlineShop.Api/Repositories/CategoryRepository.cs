using MongoDB.Driver;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IMongoCollection<Category> _categories;

    public CategoryRepository(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionURI"]);
        var db = client.GetDatabase(config["MongoDB:DatabaseName"]);
        _categories = db.GetCollection<Category>("Categories");
    }

    public async Task<List<Category>> GetAllAsync() =>
        await _categories.Find(_ => true).ToListAsync();

    public async Task<Category?> GetByIdAsync(string id) =>
        await _categories.Find(c => c.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Category category) =>
        await _categories.InsertOneAsync(category);

    public async Task<bool> UpdateAsync(Category category)
    {
        var result = await _categories.ReplaceOneAsync(c => c.Id == category.Id, category);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _categories.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}

using MongoDB.Driver;
using MongoDB.Bson;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _products;

    public ProductRepository(IConfiguration config)
    {
        var connectionString = config["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionString missing in configuration.");

        var dbName = config["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDB:DatabaseName missing in configuration.");

        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(dbName);

        _products = db.GetCollection<Product>("Products");
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task CreateAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentNullException(nameof(product.Name));

        if (product.Price <= 0)
            throw new ArgumentOutOfRangeException(nameof(product.Price));

        if (string.IsNullOrWhiteSpace(product.Id))
            product.Id = ObjectId.GenerateNewId().ToString();

        await _products.InsertOneAsync(product);
    }

    // ---------------------------------------------------------
    // READ
    // ---------------------------------------------------------

    public async Task<List<Product>> GetAllAsync() =>
        await _products.Find(_ => true).ToListAsync();

    public async Task<Product?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    public async Task<bool> UpdateAsync(Product product)
    {
        if (product == null)
            return false;

        if (string.IsNullOrWhiteSpace(product.Id))
            return false;

        if (!ObjectId.TryParse(product.Id, out _))
            return false;

        if (string.IsNullOrWhiteSpace(product.Name))
            return false;

        if (product.Price <= 0)
            return false;

        var existing = await GetByIdAsync(product.Id);
        if (existing == null)
            return false;

        var result = await _products.ReplaceOneAsync(
            p => p.Id == product.Id,
            product
        );

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

        var result = await _products.DeleteOneAsync(p => p.Id == id);

        return result.DeletedCount == 1;
    }

    // ---------------------------------------------------------
    // SEARCH + FILTERING
    // ---------------------------------------------------------

    public async Task<List<Product>> SearchAsync(
        string? keyword,
        string? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        bool? inStock,
        string? sortBy,
        bool descending
    )
    {
        var filters = new List<FilterDefinition<Product>>();

        // Keyword search (name + description)
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var regex = new MongoDB.Bson.BsonRegularExpression(keyword, "i");

            filters.Add(
                Builders<Product>.Filter.Or(
                    Builders<Product>.Filter.Regex(p => p.Name, regex),
                    Builders<Product>.Filter.Regex(p => p.Description, regex)
                )
            );
        }

        // Category filter
        if (!string.IsNullOrWhiteSpace(categoryId))
            filters.Add(Builders<Product>.Filter.Eq(p => p.CategoryId, categoryId));

        // Price range
        if (minPrice.HasValue)
            filters.Add(Builders<Product>.Filter.Gte(p => p.Price, minPrice.Value));

        if (maxPrice.HasValue)
            filters.Add(Builders<Product>.Filter.Lte(p => p.Price, maxPrice.Value));

        // In-stock filter
        if (inStock.HasValue && inStock.Value)
            filters.Add(Builders<Product>.Filter.Gt(p => p.StockQuantity, 0));

        // Combine filters
        var filter = filters.Count > 0
            ? Builders<Product>.Filter.And(filters)
            : Builders<Product>.Filter.Empty;

        var query = _products.Find(filter);

        // Sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            var sort = descending
                ? Builders<Product>.Sort.Descending(sortBy)
                : Builders<Product>.Sort.Ascending(sortBy);

            query = query.Sort(sort);
        }

        return await query.ToListAsync();
    }
}

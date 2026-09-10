using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OnlineShop.Api.Models;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("stockQuantity")]
    public int StockQuantity { get; set; }

    [BsonElement("categoryId")]
    public string? CategoryId { get; set; }
}

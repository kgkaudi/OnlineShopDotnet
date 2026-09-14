using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OnlineShop.Api.Models;

public class InvalidToken
{
    [BsonId]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Token { get; set; } = string.Empty;

    public DateTime InvalidatedAt { get; set; } = DateTime.UtcNow;
}

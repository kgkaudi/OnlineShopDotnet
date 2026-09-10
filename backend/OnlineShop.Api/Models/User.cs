using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OnlineShop.Api.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("email")]
    public string Email { get; set; } = null!;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = null!;

    [BsonElement("fullName")]
    public string? FullName { get; set; }

    [BsonElement("roles")]
    public List<string> Roles { get; set; } = new() { "User" };
}

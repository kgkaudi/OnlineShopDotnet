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

    // Shipping address
    [BsonElement("shippingAddress")]
    public Address? ShippingAddress { get; set; }

    // Billing address
    [BsonElement("billingAddress")]
    public Address? BillingAddress { get; set; }

    // Delivery contact
    [BsonElement("phoneNumber")]
    public string? PhoneNumber { get; set; }

    // Account creation timestamp
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Last profile update timestamp
    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Email verification status
    [BsonElement("isEmailVerified")]
    public bool IsEmailVerified { get; set; } = false;
}
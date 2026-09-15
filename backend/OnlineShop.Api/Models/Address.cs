using MongoDB.Bson.Serialization.Attributes;

namespace OnlineShop.Api.Models;

public class Address
{
    [BsonElement("street")]
    public string? Street { get; set; }

    [BsonElement("city")]
    public string? City { get; set; }

    [BsonElement("state")]
    public string? State { get; set; }

    [BsonElement("postalCode")]
    public string? PostalCode { get; set; }

    [BsonElement("country")]
    public string? Country { get; set; }
}
using MongoDB.Driver;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Migrations;

public class MigrationRunner
{
    private readonly IMongoDatabase _db;

    public MigrationRunner(IMongoClient client)
    {
        _db = client.GetDatabase("OnlineShopDb");
    }

    public async Task RunAsync()
    {
        await EnsureCollectionsAsync();
        await SeedData.RunAsync(_db);
    }

    private async Task EnsureCollectionsAsync()
    {
        var existing = await _db.ListCollectionNames().ToListAsync();
        var required = new[]
        {
            "Users",
            "Products",
            "Inventory",
            "Category",
            "Coupon",
            "Order",
            "Cart",
            "Wishlist",
            "Review"
        };

        foreach (var name in required)
        {
            if (!existing.Contains(name))
                await _db.CreateCollectionAsync(name);
        }
    }
}

using MongoDB.Driver;
using MongoDB.Bson;
using OnlineShop.Api.Models;

namespace OnlineShop.Api.Migrations;

public static class SeedData
{
    public static async Task RunAsync(IMongoDatabase db)
    {
        await SeedAdminUser(db);
        await SeedCategories(db);
        await SeedProducts(db);
        await SeedInventory(db);
        await SeedCoupons(db);
        await SeedWishlist(db);
        await SeedReviews(db);
        await SeedOrders(db);
    }

    private static async Task SeedAdminUser(IMongoDatabase db)
    {
        var users = db.GetCollection<User>("Users");

        if (await users.CountDocumentsAsync(FilterDefinition<User>.Empty) > 0)
            return;

        var admin = new User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = "admin@shop.com",
            FullName = "Admin User",
            Roles = new List<string> { "Admin" },
            PasswordHash = "admin" // replace with hashed version if needed
        };

        await users.InsertOneAsync(admin);
    }

    private static async Task SeedCategories(IMongoDatabase db)
    {
        var col = db.GetCollection<Category>("Category");
        if (await col.CountDocumentsAsync(FilterDefinition<Category>.Empty) > 0)
            return;

        var categories = new[]
        {
            new Category { Id = ObjectId.GenerateNewId().ToString(), Name = "Electronics" },
            new Category { Id = ObjectId.GenerateNewId().ToString(), Name = "Books" },
            new Category { Id = ObjectId.GenerateNewId().ToString(), Name = "Clothing" }
        };

        await col.InsertManyAsync(categories);
    }

    private static async Task SeedProducts(IMongoDatabase db)
    {
        var col = db.GetCollection<Product>("Products");
        if (await col.CountDocumentsAsync(FilterDefinition<Product>.Empty) > 0)
            return;

        var products = new[]
        {
            new Product
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Laptop Pro 15",
                Description = "High performance laptop",
                Price = 1499.99m,
                CategoryId = await GetCategoryId(db, "Electronics")
            },
            new Product
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Noise Cancelling Headphones",
                Description = "Premium audio experience",
                Price = 299.99m,
                CategoryId = await GetCategoryId(db, "Electronics")
            }
        };

        await col.InsertManyAsync(products);
    }

    private static async Task<string> GetCategoryId(IMongoDatabase db, string name)
    {
        var col = db.GetCollection<Category>("Category");
        var cat = await col.Find(x => x.Name == name).FirstAsync();
        return cat.Id;
    }

    private static async Task SeedInventory(IMongoDatabase db)
    {
        var products = db.GetCollection<Product>("Products")
                         .Find(FilterDefinition<Product>.Empty)
                         .ToList();

        if (products.Count == 0)
            return;

        var productCollection = db.GetCollection<Product>("Products");

        foreach (var p in products)
        {
            var update = Builders<Product>.Update.Set(x => x.StockQuantity, 50);
            await productCollection.UpdateOneAsync(x => x.Id == p.Id, update);
        }

        // Optional: seed inventory events
        var events = db.GetCollection<InventoryEvent>("InventoryEvent");

        foreach (var p in products)
        {
            await events.InsertOneAsync(new InventoryEvent
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProductId = p.Id,
                Change = +50,
                Reason = "Initial stock",
                Timestamp = DateTime.UtcNow
            });
        }
    }

    private static async Task SeedCoupons(IMongoDatabase db)
    {
        var col = db.GetCollection<Coupon>("Coupon");

        if (await col.CountDocumentsAsync(FilterDefinition<Coupon>.Empty) > 0)
            return;

        var coupons = new[]
        {
        new Coupon
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Code = "WELCOME10",
            Type = "percentage",
            Value = 10,
            Expiration = DateTime.UtcNow.AddMonths(3),
            MaxUsage = 100,
            UsedCount = 0,
            Active = true
        },
        new Coupon
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Code = "SAVE50",
            Type = "fixed",
            Value = 50,
            Expiration = DateTime.UtcNow.AddMonths(6),
            MaxUsage = 50,
            UsedCount = 0,
            Active = true
        }
    };

        await col.InsertManyAsync(coupons);
    }

    private static async Task SeedWishlist(IMongoDatabase db)
    {
        var col = db.GetCollection<WishlistItem>("Wishlist");
        if (await col.CountDocumentsAsync(FilterDefinition<WishlistItem>.Empty) > 0)
            return;

        var userId = await GetAdminId(db);
        var productId = await GetFirstProductId(db);

        await col.InsertOneAsync(new WishlistItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            ProductId = productId,
            AddedAt = DateTime.UtcNow
        });
    }

    private static async Task SeedReviews(IMongoDatabase db)
    {
        var col = db.GetCollection<Review>("Review");
        if (await col.CountDocumentsAsync(FilterDefinition<Review>.Empty) > 0)
            return;

        var userId = await GetAdminId(db);
        var productId = await GetFirstProductId(db);

        await col.InsertOneAsync(new Review
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            ProductId = productId,
            Rating = 5,
            Comment = "Excellent product!",
            CreatedAt = DateTime.UtcNow
        });
    }

    private static async Task SeedOrders(IMongoDatabase db)
    {
        var col = db.GetCollection<Order>("Order");

        if (await col.CountDocumentsAsync(FilterDefinition<Order>.Empty) > 0)
            return;

        var userId = await GetAdminId(db);
        var productId = await GetFirstProductId(db);

        await col.InsertOneAsync(new Order
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = productId,
                Quantity = 1
            }
        },
            Total = 1499.99m,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static async Task<string> GetAdminId(IMongoDatabase db)
    {
        var col = db.GetCollection<User>("Users");
        var admin = await col.Find(x => x.Roles.Contains("Admin")).FirstAsync();
        return admin.Id;
    }

    private static async Task<string> GetFirstProductId(IMongoDatabase db)
    {
        var col = db.GetCollection<Product>("Products");
        var product = await col.Find(FilterDefinition<Product>.Empty).FirstAsync();
        return product.Id;
    }
}

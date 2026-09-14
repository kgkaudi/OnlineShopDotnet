public abstract class RepositoryTestBase : IClassFixture<MongoTestFixture>
{
    protected readonly MongoTestFixture Fixture;

    protected RepositoryTestBase(MongoTestFixture fixture)
    {
        Fixture = fixture;

        // Clean collections before each test.
        // NOTE: these must match the exact names passed to GetCollection<T>(...)
        // in each repository (all plural). There is no separate "Inventory"
        // collection — InventoryRepository reads/writes "Products" directly.
        Fixture.Database.DropCollection("Carts");
        Fixture.Database.DropCollection("Categories");
        Fixture.Database.DropCollection("Coupons");
        Fixture.Database.DropCollection("Orders");
        Fixture.Database.DropCollection("Products");
        Fixture.Database.DropCollection("Reviews");
        Fixture.Database.DropCollection("Users");
        Fixture.Database.DropCollection("Wishlist");
    }
}
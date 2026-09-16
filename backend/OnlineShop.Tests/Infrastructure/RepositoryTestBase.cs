public abstract class RepositoryTestBase : IClassFixture<MongoTestFixture>
{
    protected readonly MongoTestFixture Fixture;

    protected RepositoryTestBase(MongoTestFixture fixture)
    {
        Fixture = fixture;

        // Clean collections before each test.
        // NOTE: these must match the exact names passed to GetCollection<T>(...)
        // in each repository — verified directly against the repository source,
        // not assumed. Category and Order are singular; everything else here
        // is plural. There is no separate "Inventory" collection —
        // InventoryRepository reads/writes "Products" directly.
        Fixture.Database.DropCollection("Carts");
        Fixture.Database.DropCollection("Category");
        Fixture.Database.DropCollection("Coupons");
        Fixture.Database.DropCollection("InvalidTokens");
        Fixture.Database.DropCollection("Order");
        Fixture.Database.DropCollection("Products");
        Fixture.Database.DropCollection("Reviews");
        Fixture.Database.DropCollection("Users");
        Fixture.Database.DropCollection("Wishlist");
    }
}
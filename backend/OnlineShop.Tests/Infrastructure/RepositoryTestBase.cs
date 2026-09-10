public abstract class RepositoryTestBase : IClassFixture<MongoTestFixture>
{
    protected readonly MongoTestFixture Fixture;

    protected RepositoryTestBase(MongoTestFixture fixture)
    {
        Fixture = fixture;

        // Clean collections before each test
        Fixture.Database.DropCollection("Cart");
        Fixture.Database.DropCollection("Category");
        Fixture.Database.DropCollection("Coupon");
        Fixture.Database.DropCollection("Inventory");
        Fixture.Database.DropCollection("Order");
        Fixture.Database.DropCollection("Products");
        Fixture.Database.DropCollection("Review");
        Fixture.Database.DropCollection("Users");
        Fixture.Database.DropCollection("Wishlist");
    }
}
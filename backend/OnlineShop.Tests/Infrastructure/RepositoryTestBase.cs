public abstract class RepositoryTestBase : IClassFixture<MongoTestFixture>
{
    protected readonly MongoTestFixture Fixture;

    protected RepositoryTestBase(MongoTestFixture fixture)
    {
        Fixture = fixture;

        // Clean collections before each test
        Fixture.Database.DropCollection("Products");
        Fixture.Database.DropCollection("Users");
        Fixture.Database.DropCollection("Cart");
        Fixture.Database.DropCollection("Category");
    }
}
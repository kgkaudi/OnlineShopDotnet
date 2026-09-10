public abstract class RepositoryTestBase : IClassFixture<MongoTestFixture>
{
    protected readonly MongoTestFixture Fixture;

    protected RepositoryTestBase(MongoTestFixture fixture)
    {
        Fixture = fixture;

        // Clean the Products collection before each test
        Fixture.Database.DropCollection("Products");
    }
}

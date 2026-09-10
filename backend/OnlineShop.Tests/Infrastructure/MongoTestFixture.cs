using Mongo2Go;
using MongoDB.Driver;

public class MongoTestFixture : IDisposable
{
    public MongoDbRunner Runner { get; }
    public IMongoDatabase Database { get; }

    public MongoTestFixture()
    {
        // Disable Mongo2Go logging
        Environment.SetEnvironmentVariable("MONGO2GO_LOGLEVEL", "NONE");

        Runner = MongoDbRunner.Start(singleNodeReplSet: false);
        var client = new MongoClient(Runner.ConnectionString);
        Database = client.GetDatabase("OnlineShop_TestDb");
    }

    public void Dispose()
    {
        Runner.Dispose();
    }
}


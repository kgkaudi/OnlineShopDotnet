using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

public static class TestConfiguration
{
    public static IConfiguration Create(string connectionString, string dbName)
    {
        var dict = new Dictionary<string, string?>
        {
            { "MongoDB:ConnectionString", connectionString },
            { "MongoDB:DatabaseName", dbName }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }
}

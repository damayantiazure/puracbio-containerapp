using Microsoft.Extensions.Configuration;
using System.IO;

namespace TableStorage.Abstractions.IntegrationTests.Store;

public class TestBase
{
    private const string TableStorageConnectionString = nameof(TableStorageConnectionString);
    private readonly IConfiguration _configuration;

    protected string ConnectionString => _configuration[TableStorageConnectionString];

    public TestBase()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.user.json", optional: true, reloadOnChange: true)
            .Build();
    }
}
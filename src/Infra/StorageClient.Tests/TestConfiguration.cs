#nullable enable

using Rabobank.Compliancy.Tests.Helpers;

namespace Rabobank.Compliancy.Infra.StorageClient.Tests;

public static class TestConfiguration
{
    static TestConfiguration()
    {
        ConfigurationHelper.ConfigureDefaultFiles();
    }

    public static string ConnectionString => ConfigurationHelper.GetEnvironmentVariable("AzureWebJobsStorage");
}
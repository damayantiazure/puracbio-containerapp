using Microsoft.Extensions.Configuration;

namespace Rabobank.Compliancy.Core.PipelineResources.Tests.Integration;

public class TestConfig
{
    public TestConfig()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile("appsettings.user.json", true)
            .AddEnvironmentVariables()
            .Build();

        Token = configuration["token"];
        Project = configuration["project"];
        Organization = configuration["organization"];
    }

    public string Token { get; }
    public string Project { get; }
    public string Organization { get; }
}
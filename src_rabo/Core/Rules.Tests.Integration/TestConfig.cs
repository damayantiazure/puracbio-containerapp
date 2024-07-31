using Microsoft.Extensions.Configuration;

namespace Rabobank.Compliancy.Core.Rules.Tests.Integration;

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
        ProjectId = configuration["projectid"] ?? "e31395d3-f8ac-4ed2-9555-00a330d0b97c";
        RepositoryId = configuration["repositoryid"] ?? "c819561e-2958-415a-b7e9-c9ad830a61c7";
        Organization = configuration["organization"];
    }

    public string Token { get; }
    public string Project { get; }
    public string ProjectId { get; set; }
    public string Organization { get; }
    public string RepositoryId { get; }
}
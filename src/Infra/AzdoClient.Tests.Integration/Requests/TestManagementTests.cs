using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

public class TestManagementTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public TestManagementTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(config.Organization, config.Token);
    }

    [Fact]
    public async Task GetManualTestRuns_ShouldReturnResults()
    {
        // Arrange
        var maxdate = DateTime.UtcNow;
        var mindate = maxdate.AddDays(-1);

        // Act
        var result = await _client.GetAsync(TestManagement.QueryTestRuns(_config.ProjectName, mindate, maxdate, false));

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }
}
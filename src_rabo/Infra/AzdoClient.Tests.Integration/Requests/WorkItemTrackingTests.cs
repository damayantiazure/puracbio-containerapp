using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System;
using System.Linq;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

public class WorkItemTrackingTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public WorkItemTrackingTests(TestConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _client = new AzdoRestClient(config.Organization, config.Token);
    }

    [Fact]
    public async Task GetSingle_ShouldReturnWorkItem()
    {
        // Arrange
        var queryResult = await _client.PostAsync(WorkItemTracking.QueryByWiql(_config.ProjectName, 100),
            new QueryByWiql($"select [System.Id] from WorkItems"));
        var workItem = queryResult.WorkItems.First();
        var fields = new[] { FieldNames.TeamProject };

        // Act
        var result =
            await _client.GetAsync(WorkItemTracking.GetWorkItem(_config.ProjectName, workItem.Id, fields,
                queryResult.AsOf));

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(workItem.Id);
        result.Fields.ShouldNotBeNull();
        result.Fields.ShouldContainKey(FieldNames.TeamProject);
        result.Fields[FieldNames.TeamProject].ShouldNotBeNull();
    }

    [Fact]
    public async Task QueryByWiql_ShouldReturnResults()
    {
        // Arrange

        // Act
        var result = await _client.PostAsync(WorkItemTracking.QueryByWiql(_config.ProjectName, 100),
            new QueryByWiql("select [System.Id] from WorkItems"));

        // Assert
        result.ShouldNotBeNull();
        result.WorkItems.ShouldNotBeEmpty();
        result.WorkItems.First().Id.ShouldNotBe(default);
    }
}
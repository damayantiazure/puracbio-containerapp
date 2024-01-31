using AutoFixture;
using NSubstitute;
using Rabobank.Compliancy.Infra.AzdoClient.Processors;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Processors;

public class QueryBatchProcessorTests
{
    private readonly IAzdoRestClient _client = Substitute.For<IAzdoRestClient>();
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task QueryByWiql_WithoutWhereClause_ShouldBeRunInBatchesOfSpecifiedSize()
    {
        // Arrange
        var projectName = _fixture.Create<string>();
        var organization = _fixture.Create<string>();
        var workItemReferences = new[] { new WorkItemReference { Id = 42 }, new WorkItemReference { Id = 96 } };
        var processor = new QueryBatchProcessor(_client);

        MockQueryResultsByOne(workItemReferences);

        // Act
        var results = (await processor.QueryByWiqlAsync(organization, projectName, batchSize: 1)).ToImmutableList();

        // Assert
        results.ShouldBe(workItemReferences);
        await _client
            .Received(workItemReferences.Length + 1)
            .PostAsync(
                Arg.Is<IAzdoRequest<QueryByWiql, WorkItemQueryResult>>(r =>
                    r.QueryParams.Any(p => p.Key == "$top" && 1.Equals(p.Value))),
                Arg.Any<QueryByWiql>(), organization, true);
        AssertQueriesReceivedContain(workItemReferences, "SELECT [System.Id]");
    }

    [Fact]
    public async Task QueryByWiql_WithWhereClause_ShouldBeRunInBatchesOfSpecifiedSize()
    {
        // Arrange
        const string whereClause = "[System.ChangedDate] < @startOfDay";

        var projectName = _fixture.Create<string>();
        var organization = _fixture.Create<string>();

        var workItemReferences = _fixture.CreateMany<WorkItemReference>().ToImmutableList();
        var processor = new QueryBatchProcessor(_client);

        MockQueryResultsByOne(workItemReferences);

        // Act
        var results = (await processor.QueryByWiqlAsync(organization, projectName, whereClause, 1)).ToImmutableList();

        // Assert
        results.ShouldBe(workItemReferences);
        AssertQueriesReceivedContain(workItemReferences, whereClause);
    }

    private void MockQueryResultsByOne(IEnumerable<WorkItemReference> workItemReferences)
    {
        var results = workItemReferences
            .Select(r => CreateWorkItemQueryResult(r))
            .Append(CreateWorkItemQueryResult())
            .ToImmutableList();

        _client
            .PostAsync(Arg.Any<IAzdoRequest<QueryByWiql, WorkItemQueryResult>>(), Arg.Any<QueryByWiql>(), Arg.Any<string>(), Arg.Any<bool>())
            .Returns(results[0], results.Skip(1).ToArray());
    }

    private WorkItemQueryResult CreateWorkItemQueryResult(params WorkItemReference[] workItems) =>
        new()
        {
            WorkItems = workItems.ToImmutableList(),
            AsOf = _fixture.Create<DateTime>()
        };

    private void AssertQueriesReceivedContain(ICollection<WorkItemReference> workItemReferences, string query)
    {
        _client
            .Received(workItemReferences.Count + 1)
            .PostAsync(
                Arg.Any<IAzdoRequest<QueryByWiql, WorkItemQueryResult>>(),
                Arg.Is<QueryByWiql>(q => QueryContains(q, query)), Arg.Any<string>(), Arg.Any<bool>());
    }

    private static bool QueryContains(QueryByWiql queryByWiql, string expectedPart) =>
        queryByWiql.Query.Contains(expectedPart, StringComparison.InvariantCultureIgnoreCase);
}
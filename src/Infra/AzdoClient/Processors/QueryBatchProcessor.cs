using System.Globalization;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Infra.AzdoClient.Processors;

public class QueryBatchProcessor : IQueryBatchProcessor
{
    private const int DefaultBatchSize = 20_000 - 1;

    private readonly IAzdoRestClient _client;

    public QueryBatchProcessor(IAzdoRestClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<IEnumerable<WorkItemReference>> QueryByWiqlAsync(string organization, string project, string whereClause = null,
        int batchSize = DefaultBatchSize)
    {
        bool hasNext;
        var workItems = new List<WorkItemReference>();

        var extraWhereClause = string.IsNullOrEmpty(whereClause) || whereClause.StartsWith("AND", true, CultureInfo.InvariantCulture)
            ? whereClause
            : $"AND {whereClause}";

        do
        {
            var start = workItems.Any() ? workItems.Last().Id : 0;
            var result = await QuerySingleBatchAsync(organization, project, start, extraWhereClause, batchSize);
            workItems.AddRange(result.WorkItems);
            hasNext = result.WorkItems.Count == batchSize;
        } while (hasNext);

        return workItems;
    }

    private Task<WorkItemQueryResult> QuerySingleBatchAsync(string organization, string project, int start, string extraWhereClause,
        int batchSize = DefaultBatchSize)
    {
        var fullWhereClause = $@"[{FieldNames.TeamProject}] = @Project AND [{FieldNames.Id}] > {
            start
        } {extraWhereClause} ORDER BY [{FieldNames.Id}]";
        var query = $"SELECT [{FieldNames.Id}] FROM WorkItems WHERE {fullWhereClause}";

        return _client.PostAsync(WorkItemTracking.QueryByWiql(project, batchSize), new QueryByWiql(query), organization, true);
    }
}
#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public class DeviationStorageRepository : IDeviationStorageRepository
{
    private const string _tableName = "Deviations";
    private readonly Lazy<CloudTable> _lazyDeviationsTable;

    public DeviationStorageRepository(Func<CloudTableClient> factory) =>
        _lazyDeviationsTable = new Lazy<CloudTable>(() =>
            GetTable(factory()));

    public async Task<IList<Deviation>> GetListAsync(string organization, string projectId)
    {
        var query = new TableQuery<Deviation>().Where(TableQuery.CombineFilters(
            TableQuery.GenerateFilterCondition("Organization", QueryComparisons.Equal, organization),
            TableOperators.And,
            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, projectId)));

        var items = new List<Deviation>();
        TableContinuationToken? token = null;
        do
        {
            var results = await _lazyDeviationsTable.Value.ExecuteQuerySegmentedAsync(query, token);
            items.AddRange(results);
            token = results.ContinuationToken;
        } while (token != null);

        return items;
    }

    public async Task UpdateAsync(Deviation deviation) =>
        await _lazyDeviationsTable.Value.ExecuteAsync(TableOperation.InsertOrReplace(deviation));

    public async Task DeleteAsync(DynamicTableEntity deviation) =>
        await _lazyDeviationsTable.Value.ExecuteAsync(TableOperation.Delete(deviation));

    private static CloudTable GetTable(CloudTableClient client)
    {
        var table = client.GetTableReference(_tableName);
        table.CreateIfNotExists();
        return table;
    }
}
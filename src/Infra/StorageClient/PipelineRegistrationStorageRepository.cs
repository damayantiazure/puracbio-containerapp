#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Rabobank.Compliancy.Infra.StorageClient.Model;

namespace Rabobank.Compliancy.Infra.StorageClient;

public class PipelineRegistrationStorageRepository : IPipelineRegistrationStorageRepository
{
    private const string _prodPartitionKey = "PROD";
    private const string _nonProdPartitionKey = "NON-PROD";
    private const string _tableName = "DeploymentMethod";
    private readonly Lazy<CloudTable> _lazyPipelineRegistrationTable;
    private readonly ILogger<PipelineRegistrationStorageRepository> _logger;

    public PipelineRegistrationStorageRepository(
        ILogger<PipelineRegistrationStorageRepository> logger, Func<CloudTableClient> factory)
    {
        _lazyPipelineRegistrationTable = new Lazy<CloudTable>(() => GetTable(factory()));
        _logger = logger;
    }

    public async Task ImportAsync(IEnumerable<PipelineRegistration> items)
    {
        var prodItems = items.Where(e =>
                !string.IsNullOrEmpty(e.CiIdentifier) &&
                e is { PipelineId: not null, ProjectId: not null })
            .Distinct()
            .ToList();
        await AddBatchAsync(prodItems);
    }

    public async Task ClearAsync()
    {
        var query = new TableQuery<PipelineRegistration>()
            .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _prodPartitionKey))
            .Select(new[] { "PartitionKey", "RowKey" });

        TableContinuationToken? continuationToken = null;
        do
        {
            var tableQueryResult = await _lazyPipelineRegistrationTable.Value
                .ExecuteQuerySegmentedAsync(query, continuationToken);
            continuationToken = tableQueryResult.ContinuationToken;
            await ExecuteBatchAsync(_lazyPipelineRegistrationTable.Value, tableQueryResult, DeleteAction());
        } while (continuationToken != null);
    }

    public async Task AddBatchAsync(IEnumerable<PipelineRegistration> items)
    {
        await ExecuteBatchAsync(_lazyPipelineRegistrationTable.Value, items, InsertAction());
        // Remove non-prod registrations when a prod registration is found for the same pipelineId and pipelineType
        await RemoveDuplicates(items);
        await RemoveRegistrationsNotPresentInSm9(items);
    }

    public async Task DeleteEntityAsync(string ciIdentifier, string projectId,
        string pipelineId, string pipelineType, string stageId)
    {
        var partitionKey = PipelineRegistration
            .CreatePartitionKey(ciIdentifier);
        var rowKey = PipelineRegistration
            .CreateRowKey(ciIdentifier, projectId, pipelineId, pipelineType, stageId);
        var item = await ExecuteTableOperationAsync<PipelineRegistration>(
            TableOperation.Retrieve<PipelineRegistration>(partitionKey, rowKey));
        await DeleteEntityAsync(item);
    }

    public async Task DeleteEntityAsync(PipelineRegistration? item)
    {
        if (item != null)
        {
            await ExecuteTableOperationAsync<PipelineRegistration>(TableOperation.Delete(item));
        }
    }

    public async Task DeleteEntitiesForPipelineAsync(
        string? ciIdentifier, string projectId, string pipelineId, string pipelineType, string? stageId)
    {
        var partitionKey = PipelineRegistration.CreatePartitionKey(ciIdentifier);
        var rowKeyPart = PipelineRegistration.CreateRowKey(ciIdentifier, projectId, pipelineId, pipelineType, stageId);
        var partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
        var rowKeyFilter = GetStartsWithFilter("RowKey", rowKeyPart);
        var query = new TableQuery<PipelineRegistration>().Where(TableQuery.CombineFilters(partitionFilter,
            TableOperators.And, rowKeyFilter));

        var entities = await GetPipelineRegistrations(query);

        foreach (var entity in entities)
        {
            await DeleteEntityAsync(entity);
        }
    }

    public async Task<PipelineRegistration?> InsertOrMergeEntityAsync(PipelineRegistration item) =>
        await ExecuteTableOperationAsync<PipelineRegistration>(TableOperation.InsertOrMerge(item));

    private async Task RemoveDuplicates(IEnumerable<PipelineRegistration> prodItems)
    {
        var nonProdPipelineRegistrations = await GetPipelineRegistrations(_nonProdPartitionKey,
            new[] { "PartitionKey", "RowKey", "PipelineId", "PipelineType", "Organization", "ProjectId" });

        var duplicates = nonProdPipelineRegistrations
            .Where(r => prodItems.Any(p => p.PipelineId == r.PipelineId && p.PipelineType == r.PipelineType &&
                                           p.Organization == r.Organization && p.ProjectId == r.ProjectId));

        await ExecuteBatchAsync(_lazyPipelineRegistrationTable.Value, duplicates, DeleteAction());
    }

    private async Task RemoveRegistrationsNotPresentInSm9(IEnumerable<PipelineRegistration> prodItems)
    {
        var prodPipelineRegistrations = await GetPipelineRegistrations(_prodPartitionKey,
            new[] { "PartitionKey", "RowKey", "PipelineId", "PipelineType", "Organization", "ProjectId", "StageId" });

        var registrationsNotInSm9 = new List<PipelineRegistration>();

        foreach (var reg in prodPipelineRegistrations)
        {
            if (!prodItems.Any(p => p.Equals(reg) && p.Organization == reg.Organization))
            {
                registrationsNotInSm9.Add(reg);
            }
        }

        await ExecuteBatchAsync(_lazyPipelineRegistrationTable.Value, registrationsNotInSm9, DeleteAction());
    }

    private async Task<IEnumerable<PipelineRegistration>> GetPipelineRegistrations(string partitionKey, string[] fields)
    {
        var query = new TableQuery<PipelineRegistration>()
            .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey))
            .Select(fields);

        return await GetPipelineRegistrations(query);
    }

    private async Task<IEnumerable<PipelineRegistration>> GetPipelineRegistrations(
        TableQuery<PipelineRegistration> query)
    {
        var pipelineRegistrations = new List<PipelineRegistration>();
        TableContinuationToken? continuationToken = null;
        do
        {
            var page = await _lazyPipelineRegistrationTable.Value.ExecuteQuerySegmentedAsync(query, continuationToken);
            continuationToken = page.ContinuationToken;
            pipelineRegistrations.AddRange(page.Results);
        } while (continuationToken != null);

        return pipelineRegistrations;
    }

    private async Task<T?> ExecuteTableOperationAsync<T>(TableOperation operation)
        where T : TableEntity
    {
        try
        {
            var result = await _lazyPipelineRegistrationTable.Value.ExecuteAsync(operation);
            return result?.Result as T;
        }
        catch (StorageException e)
        {
            _logger.LogError(
                $"Error executing operation {operation.OperationType} on table '{_tableName}' " +
                $"with entity with PartitionKey='{operation.Entity.PartitionKey}' and " +
                $"RowKey='{operation.Entity.RowKey}'", e);
            throw;
        }
    }

    private static Action<TableBatchOperation, ITableEntity> DeleteAction() =>
        (bo, item) => bo.Delete(item);

    private static Action<TableBatchOperation, ITableEntity> InsertAction() =>
        (bo, item) => bo.InsertOrReplace(item);

    private static async Task ExecuteBatchAsync(
        CloudTable table,
        IEnumerable<ITableEntity> items,
        Action<TableBatchOperation, ITableEntity> operation)
    {
        // only put items with same partitionKey in a batch 
        var batches = items.GroupBy(i => i.PartitionKey);
        foreach (var batch in batches)
        {
            var chunks = batch.ToChunksOf(100);
            foreach (var chunk in chunks)
            {
                var batchOperation = new TableBatchOperation();
                foreach (var item in chunk)
                {
                    operation(batchOperation, item);
                }

                await table.ExecuteBatchAsync(batchOperation);
            }
        }
    }

    private static CloudTable GetTable(CloudTableClient client)
    {
        var table = client.GetTableReference(_tableName);
        table.CreateIfNotExists();
        return table;
    }

    private static string GetStartsWithFilter(string columnName, string startsWith)
    {
        var length = startsWith.Length - 1;
        var nextChar = startsWith[length] + 1;

        var startWithEnd = startsWith[..length] + (char)nextChar;
        var filter = TableQuery.CombineFilters(
            TableQuery.GenerateFilterCondition(columnName, QueryComparisons.GreaterThanOrEqual, startsWith),
            TableOperators.And,
            TableQuery.GenerateFilterCondition(columnName, QueryComparisons.LessThan, startWithEnd));

        return filter;
    }
}
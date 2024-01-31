#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace Rabobank.Compliancy.Infra.StorageClient;

public class StorageRepository : IStorageRepository
{
    private readonly Lazy<CloudTableClient> _lazyClient;
    private CloudTable? _table;

    public StorageRepository(Func<CloudTableClient> factory) =>
        _lazyClient = new Lazy<CloudTableClient>(factory);

    public async Task<TableResult?> GetEntityAsync(string? partitionKey, string? rowKey)
    {
        if (_table == null)
        {
            throw new InvalidOperationException("Cloud table not initialized.");
        }

        if (string.IsNullOrEmpty(partitionKey))
        {
            throw new InvalidOperationException("Partition key is null or empty");
        }

        if (string.IsNullOrEmpty(rowKey))
        {
            throw new InvalidOperationException("Row key is null or empty");
        }

        return await _table.ExecuteAsync(TableOperation.Retrieve(partitionKey, rowKey));
    }

    public async Task<TableResult?> GetEntityAsync<TResponse>(string? partitionKey, string? rowKey)
        where TResponse : ITableEntity
    {
        if (_table == null)
        {
            throw new InvalidOperationException("Cloud table not initialized.");
        }

        if (string.IsNullOrEmpty(partitionKey))
        {
            throw new InvalidOperationException("Partition key is null or empty");
        }

        if (string.IsNullOrEmpty(rowKey))
        {
            throw new InvalidOperationException("Row key is null or empty");
        }

        return await _table.ExecuteAsync(TableOperation.Retrieve<TResponse>(partitionKey, rowKey));
    }

    public async Task<TableResult> InsertOrReplaceAsync(DynamicTableEntity entity)
    {
        if (_table == null)
        {
            throw new InvalidOperationException("Cloud table not initialized.");
        }

        return await _table.ExecuteAsync(TableOperation.InsertOrReplace(entity));
    }

    public async Task InsertOrReplaceAsync<TResponse>(IList<TResponse> entities) where TResponse : ITableEntity
    {
        if (_table == null)
        {
            throw new InvalidOperationException("Cloud table not initialized.");
        }

        await Task.WhenAll(entities.Select(async entity => 
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(entity))));
    }

    public async Task<TableResult> InsertOrMergeAsync(ITableEntity entity)
    {
        if (_table == null)
        {
            throw new InvalidOperationException("Cloud table not initialized.");
        }

        return await _table.ExecuteAsync(TableOperation.InsertOrMerge(entity));
    }

    public async Task<TableResult> DeleteAsync(DynamicTableEntity entity)
    {
        if (_table == null)
        {
            throw new InvalidOperationException("Cloud table not initialized.");
        }

        return await _table.ExecuteAsync(TableOperation.Delete(entity));
    }

    public async Task DeleteAllRowsAsync<TResponse>() where TResponse : ITableEntity, new()
    {
        if (_table == null)
        {
            throw new InvalidOperationException("Cloud table not initialized.");
        }

        var query = new TableQuery<TResponse>();
        TableContinuationToken? token = null;

        do
        {
            var result = await _table.ExecuteQuerySegmentedAsync(query, token);

            await Task.WhenAll(result
                .Select(async row => await _table.ExecuteAsync(TableOperation.Delete(row))));

            token = result.ContinuationToken;
        } while (token != null);
    }

    public DynamicTableEntity CreateTableEntity(string? partitionKey, string? rowKey) =>
        new(partitionKey, rowKey)
        {
            ETag = "*"
        };

    public string GetTableName()
    {
        if (_table == null)
        {
            throw new InvalidOperationException("Cloud table not initialized.");
        }

        return _table.Name;
    }

    public CloudTable CreateTable(string tableName)
    {
        _table = _lazyClient.Value.GetTableReference(tableName);
        _table.CreateIfNotExistsAsync();
        return _table;
    }
}
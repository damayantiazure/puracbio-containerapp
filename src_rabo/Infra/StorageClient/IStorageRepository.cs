#nullable enable

using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Infra.StorageClient;

public interface IStorageRepository
{
    Task<TableResult?> GetEntityAsync(string? partitionKey, string? rowKey);

    Task<TableResult?> GetEntityAsync<TResponse>(string? partitionKey, string? rowKey) where TResponse : ITableEntity;

    Task InsertOrReplaceAsync<TResponse>(IList<TResponse> entities) where TResponse : ITableEntity;

    Task<TableResult> InsertOrReplaceAsync(DynamicTableEntity entity);

    Task<TableResult> InsertOrMergeAsync(ITableEntity entity);

    Task DeleteAllRowsAsync<TResponse>() where TResponse : ITableEntity, new();

    DynamicTableEntity CreateTableEntity(string? partitionKey, string? rowKey);

    Task<TableResult> DeleteAsync(DynamicTableEntity entity);

    string GetTableName();

    CloudTable CreateTable(string tableName);
}
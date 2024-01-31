using Azure;
using Azure.Data.Tables;

namespace Rabobank.Compliancy.Clients.AzureDataTablesClient;

public abstract class BaseEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    /// <summary>
    /// This constructor is used when reading the entity from the table storage. When creating a new entity to insert
    /// please use the parameterized constructor.
    /// </summary>
    protected BaseEntity()
    {
        RowKey = string.Empty;
        PartitionKey = string.Empty;
    }

    /// <summary>
    /// Used to create a new entity. This means a partitionKey and RowKeyParameters need to be provided.
    /// </summary>
    /// <param name="partitionKey"></param>
    /// <param name="rowKeyParameters"></param>
    protected BaseEntity(object partitionKey, params object?[] rowKeyParameters)
    {
        RowKey = RowKeyGenerator.GenerateRowKey(rowKeyParameters);
        PartitionKey = partitionKey.ToString() ?? throw new ArgumentNullException(nameof(partitionKey));
    }

    /// <summary>
    /// Used to create a new entity. This means a partitionKey and RowKeyParameters need to be provided.
    /// </summary>
    protected BaseEntity(string partitionKey, string? ciIdentifier, Guid projectId, int pipelineId, string pipelineType, string stageId)
    {
        RowKey = RowKeyGenerator.GenerateVerySpecialRowKey(ciIdentifier, projectId.ToString(), pipelineId.ToString(), pipelineType, stageId);
        PartitionKey = partitionKey;
    }
}
using Microsoft.Azure.Cosmos.Table;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;

public class PreviewFeature : TableEntity
{
    public PreviewFeature()
    {

    }

    public PreviewFeature(string featureName, string projectId)
    {
        PartitionKey = featureName;
        RowKey = $"{projectId}";
    }

    public string Organization { get; set; }
    public string ProjectName { get; set; }
    public string ProjectId { get; set; }
}
using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;

public class Deviation : TableEntity
{
    public Deviation()
    {
        // intentionally left empty
    }

    public Deviation(
        string organization,
        string projectName,
        string ruleName,
        string itemId,
        string ciIdentifier,
        string projectId,
        string foreignProjectId)
    {
        Organization = organization;
        ProjectName = projectName;
        ProjectId = projectId;
        RuleName = ruleName;
        ItemId = itemId;
        Date = DateTime.UtcNow;
        PartitionKey = projectId;
        CiIdentifier = ciIdentifier;
        ForeignProjectId = foreignProjectId;
        RowKey = CreateRowKey(organization, projectId, ruleName, itemId, ciIdentifier, foreignProjectId);
    }

    public string Organization { get; set; }
    public string ProjectName { get; set; }
    public string ProjectId { get; set; }
    public string RuleName { get; set; }
    public string ItemId { get; set; }
    public string Comment { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime Date { get; set; }
    public string Reason { get; set; }
    public string ReasonNotApplicable { get; set; }
    public string ReasonNotApplicableOther { get; set; }
    public string ReasonOther { get; set; }
    public string CiIdentifier { get; set; }
    public string ForeignProjectId { get; set; }

    public static string CreateRowKey(
        string organization, 
        string projectId,
        string ruleName,
        string itemId,
        string ciIdentifier,
        string foreignProjectId) =>
        CreateMd5($"{organization}_{projectId}_{ruleName}_{itemId}_{ciIdentifier}_{foreignProjectId}");
        
    public override string ToString() =>
        $"PartitionKey:{PartitionKey}|RowKey:{RowKey}";

    private static string CreateMd5(string input) =>
        BitConverter.ToString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(input)))
            .Replace("-", string.Empty)
            .ToLower();
}
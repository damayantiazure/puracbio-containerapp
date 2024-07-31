using Microsoft.Azure.Cosmos.Table;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;

public class Exclusion : TableEntity
{
    public Exclusion()
    {

    }
    public Exclusion(PipelineRunInfo runInfo)
    {
        PartitionKey = "Exclusion";
        RowKey = CreateRowKey(runInfo);
    }

    public string Organization { get; set; }
    public string ProjectId { get; set; }
    public string PipelineId { get; set; }
    public string PipelineType { get; set; }
    public string ExclusionReasonRequester { get; set; }
    public string Requester { get; set; }
    public string ExclusionReasonApprover { get; set; }
    public string Approver { get; set; }
    public string RunId { get; set; }
    internal static string CreateRowKey(PipelineRunInfo runInfo) =>
        BitConverter.ToString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(
                $"{runInfo.Organization}_{runInfo.ProjectId}_{runInfo.PipelineId}_{runInfo.PipelineType}")))
            .Replace("-", string.Empty)
            .ToLower();
        
    public bool IsExpired(byte hoursValid) =>
        Timestamp.AddHours(hoursValid) < DateTime.Now;

    public bool IsApproved =>
        (!String.IsNullOrEmpty(Requester) && !String.IsNullOrEmpty(Approver)
                                          && Requester.ToLowerInvariant() != Approver.ToLowerInvariant());

    public bool IsNotConsumedOrIsCurrentRun(string runId) =>
        RunId == null || (!String.IsNullOrEmpty(RunId) && RunId == runId);
}
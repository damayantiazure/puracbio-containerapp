#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class PipelineBreakerReport
{
    public DateTime Date { get; set; }
    public string? Organization { get; set; }
    public string? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? PipelineId { get; set; }
    public string? PipelineName { get; set; }
    public string? PipelineType { get; set; }
    public string? PipelineVersion { get; set; }
    public string? RunId { get; set; }
    public string? RunUrl { get; set; }
    public string? StageId { get; set; }

    [Display(Name = "IsExcluded_b")]
    public bool IsExcluded { get; set; }

    public string? Requester { get; set; }
    public string? ExclusionReasonRequester { get; set; }
    public string? Approver { get; set; }
    public string? ExclusionReasonApprover { get; set; }
    public string? CiName { get; set; }
    public string? CiIdentifier { get; set; }

    [Display(Name = "RuleCompliancyReports_s")]
    public IEnumerable<RuleCompliancyReport>? RuleCompliancyReports { get; set; }

    [Display(Name = "Result_d")]
    public PipelineBreakerResult Result { get; set; }

    public bool IsValidResult(byte hoursValid, bool isBlockingEnabled) =>
        (Result == PipelineBreakerResult.Passed &&
         RuleCompliancyReports != null &&
         RuleCompliancyReports.Any() &&
         RuleCompliancyReports.All(r => r.IsDeterminedCompliant())) ||
        (IsExcluded && Date.AddHours(hoursValid) >= DateTime.Now) ||
        (Result == PipelineBreakerResult.Warned &&
         !isBlockingEnabled);
}
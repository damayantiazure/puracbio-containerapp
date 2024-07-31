#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class NonProdCompliancyReport
{
    public string? PipelineId { get; set; }
    public string? PipelineName { get; set; }
    public string? PipelineType { get; set; }
    public Uri? RescanUrl { get; set; }
    public bool IsCompliant { get; set; }
    public DateTime Date { get; set; }
    public IEnumerable<PrincipleReport>? PrincipleReports { get; set; }
    public bool IsScanFailed { get; set; }
    public ExceptionSummaryReport? ScanException { get; set; }
}
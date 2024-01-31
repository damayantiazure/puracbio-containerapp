#nullable enable

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;

[ExcludeFromCodeCoverage]
public class NonProdCompliancyReportDto
{
    [JsonProperty("pipelineId")]
    public string? PipelineId { get; set; }

    [JsonProperty("pipelineName")]
    public string? PipelineName { get; set; }

    [JsonProperty("pipelineType")]
    public string? PipelineType { get; set; }

    [JsonProperty("rescanUrl")]
    public Uri? RescanUrl { get; set; }

    [JsonProperty("isCompliant")]
    public bool IsCompliant { get; set; }

    [JsonProperty("date")]
    public DateTime Date { get; set; }

    [JsonProperty("principleReports")]
    public IEnumerable<PrincipleReportDto>? PrincipleReports { get; set; }

    [JsonProperty("isScanFailed")]
    public bool IsScanFailed { get; set; }

    [JsonProperty("scanException")]
    public ExceptionSummaryDto? ScanException { get; set; }
}
#nullable enable

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;

[ExcludeFromCodeCoverage]
public class CiReportDto
{
    [JsonProperty("aicRating")]
    public string? AicRating { get; set; }

    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("ciSubtype")]
    public string? CiSubtype { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("assignmentGroup")]
    public string? AssignmentGroup { get; set; }

    [JsonProperty("isSOx")]
    public bool IsSOx { get; set; }

    [JsonProperty("isCompliant")]
    public bool IsCompliant { get; set; }

    [JsonProperty("isSOxCompliant")]
    public bool IsSOxCompliant { get; set; }

    [JsonProperty("principleReports")]
    public IEnumerable<PrincipleReportDto>? PrincipleReports { get; set; }

    [JsonProperty("rescanUrl")]
    public Uri? RescanUrl { get; set; }

    [JsonProperty("date")]
    public DateTime Date { get; set; }

    [JsonProperty("isScanFailed")]
    public bool IsScanFailed { get; set; }

    [JsonProperty("scanException")]
    public ExceptionSummaryDto? ScanException { get; set; }

    [JsonProperty("hasDeviation")]
    public bool HasDeviation { get; set; }
}
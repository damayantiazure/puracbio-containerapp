#nullable enable

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;

[ExcludeFromCodeCoverage]
public class ItemReportDto
{
    [JsonProperty("id")]
    public string? ItemId { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("link")]
    public string? Link { get; set; }

    [JsonProperty("projectId")]
    public string? ProjectId { get; set; }

    [JsonProperty("isCompliant")]
    public bool IsCompliant { get; set; }

    [JsonProperty("isCompliantForRule")]
    public bool IsCompliantForRule { get; set; }

    [JsonProperty("reconcileUrl")]
    public Uri? ReconcileUrl { get; set; }

    [JsonProperty("rescanUrl")]
    public Uri? RescanUrl { get; set; }

    [JsonProperty("registerDeviationUrl")]
    public Uri? RegisterDeviationUrl { get; set; }

    [JsonProperty("deleteDeviationUrl")]
    public Uri? DeleteDeviationUrl { get; set; }

    [JsonProperty("reconcileImpact")]
    public string[]? ReconcileImpact { get; set; }

    [JsonProperty("date")]
    public DateTime Date { get; set; }

    [JsonProperty("deviation")]
    public DeviationReportDto? Deviation { get; set; }

    [JsonProperty("hasDeviation")]
    public bool HasDeviation { get; set; }
}
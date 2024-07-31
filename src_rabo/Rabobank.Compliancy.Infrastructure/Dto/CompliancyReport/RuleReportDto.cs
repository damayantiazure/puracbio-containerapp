#nullable enable

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;

[ExcludeFromCodeCoverage]
public class RuleReportDto
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("isCompliant")]
    public bool IsCompliant { get; set; }

    [JsonProperty("documentationUrl")]
    public Uri? DocumentationUrl { get; set; }

    [JsonProperty("itemReports")]
    public IEnumerable<ItemReportDto>? ItemReports { get; set; }

    [JsonProperty("hasDeviation")]
    public bool HasDeviation { get; set; }

    [JsonProperty("date")]
    public DateTime Date { get; set; }
}
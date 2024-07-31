#nullable enable

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;

[ExcludeFromCodeCoverage]
public class PrincipleReportDto
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("isSox")]
    public bool IsSox { get; set; }

    [JsonProperty("hasRulesToCheck")]
    public bool HasRulesToCheck { get; set; }

    [JsonProperty("isCompliant")]
    public bool IsCompliant { get; set; }

    [JsonProperty("date")]
    public DateTime Date { get; set; }

    [JsonProperty("ruleReports")]
    public IList<RuleReportDto>? RuleReports { get; set; }

    [JsonProperty("hasDeviation")]
    public bool HasDeviation { get; set; }
}
#nullable enable

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;

[ExcludeFromCodeCoverage]
public class DeviationReportDto
{
    [JsonProperty("comment")]
    public string? Comment { get; set; }

    [JsonProperty("reason")]
    public string? Reason { get; set; }

    [JsonProperty("reasonNotApplicable")]
    public string? ReasonNotApplicable { get; set; }

    [JsonProperty("reasonNotApplicableOther")]
    public string? ReasonNotApplicableOther { get; set; }

    [JsonProperty("reasonOther")]
    public string? ReasonOther { get; set; }

    [JsonProperty("updatedBy")]
    public string? UpdatedBy { get; set; }
}
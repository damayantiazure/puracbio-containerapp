#nullable enable

using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;

[ExcludeFromCodeCoverage]
public class RegistrationReportDto
{
    [JsonProperty("ciId")]
    public string? CiId { get; set; }
    [JsonProperty("ciName")]
    public string? CiName { get; set; }
    [JsonProperty("stageId")]
    public string? StageId { get; set; }
}
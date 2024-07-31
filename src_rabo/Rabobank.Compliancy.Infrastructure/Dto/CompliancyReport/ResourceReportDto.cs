#nullable enable

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;

[ExcludeFromCodeCoverage]
public class ResourceReportDto
{
    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("link")]
    public string? Link { get; set; }

    [JsonProperty("isProduction")]
    public bool IsProduction { get; set; }

    [JsonProperty("ciIdentifiers")]
    public string? CiIdentifiers { get; set; }

    [JsonProperty("openPermissionsUrl")]
    public Uri? OpenPermissionsUrl { get; set; }

    [JsonProperty("documentationUrl")]
    public Uri? DocumentationUrl { get; set; }
}
#nullable enable

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;

[ExcludeFromCodeCoverage]
public class PipelineReportDto
{
    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("productionStages")]
    public string? ProductionStages { get; set; }

    [JsonProperty("link")]
    public string? Link { get; set; }

    [JsonProperty("registrations")]
    public IEnumerable<RegistrationReportDto>? Registrations { get; set; }

    [JsonProperty("stages")]
    public IEnumerable<StageReportDto>? Stages { get; set; }

    [JsonProperty("registerUrl")]
    public Uri? RegisterUrl { get; set; }

    [JsonProperty("isProduction")]
    public bool? IsProduction { get; set; }

    [JsonProperty("ciIdentifiers")]
    public string? CiIdentifiers { get; set; }

    [JsonProperty("ruleProfileName")]
    public string? RuleProfileName { get; set; }

    [JsonProperty("ciNames")]
    public string? CiNames { get; set; }

    [JsonProperty("assignmentGroups")]
    public string? AssignmentGroups { get; set; }

    [JsonProperty("openPermissionsUrl")]
    public Uri? OpenPermissionsUrl { get; set; }

    [JsonProperty("addNonProdPipelineToScanUrl")]
    public Uri? AddNonProdPipelineToScanUrl { get; set; }

    [JsonProperty("exclusionListUrl")]
    public Uri? ExclusionListUrl { get; set; }

    [JsonProperty("updateRegistrationUrl")]
    public Uri? UpdateRegistrationUrl { get; set; }

    [JsonProperty("deleteRegistrationUrl")]
    public Uri? DeleteRegistrationUrl { get; set; }
}
#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class PipelineReport
{
    public PipelineReport(string id, string name, string type, string link, bool? isProduction)
    {
        Id = id;
        Name = name;
        Type = type;
        Link = link;
        IsProduction = isProduction;
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string? ProductionStages { get; set; }
    public string Link { get; set; }
    public IEnumerable<StageReport>? Stages { get; set; }
    public IEnumerable<RegistrationPipelineReport>? Registrations { get; set; }
    public Uri? RegisterUrl { get; set; }
    public bool? IsProduction { get; set; }
    public string? CiIdentifiers { get; set; }
    public string? RuleProfileName { get; set; }
    public string? CiNames { get; set; }
    public string? AssignmentGroups { get; set; }
    public Uri? OpenPermissionsUrl { get; set; }
    public Uri? AddNonProdPipelineToScanUrl { get; set; }
    public Uri? ExclusionListUrl { get; set; }
    public Uri? UpdateRegistrationUrl { get; set; }
    public Uri? DeleteRegistrationUrl { get; set; }
}
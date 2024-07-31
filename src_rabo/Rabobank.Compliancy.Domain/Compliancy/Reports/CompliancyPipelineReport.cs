#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class CompliancyPipelineReport
{
    [Display(Name = "organization_s")]
    public string? Organization { get; set; }

    [Display(Name = "projectId_g")]
    public string? ProjectId { get; set; }

    [Display(Name = "projectName_s")]
    public string? ProjectName { get; set; }

    [Display(Name = "pipelineId_s")]
    public string? PipelineId { get; set; }

    [Display(Name = "scanDate_t")]
    public DateTime ScanDate { get; set; }

    [Display(Name = "ciNames_s")]
    public string? CiNames { get; set; }

    [Display(Name = "assignmentGroups_s")]
    public string? AssignmentGroups { get; set; }

    [Display(Name = "registrationStatus_s")]
    public string? RegistrationStatus { get; set; }

    [Display(Name = "pipelineName_s")]
    public string? PipelineName { get; set; }

    [Display(Name = "pipelineType_s")]
    public string? PipelineType { get; set; }

    [Display(Name = "pipelineUrl_s")]
    public string? PipelineUrl { get; set; }
}
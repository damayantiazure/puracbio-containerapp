#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class PipelineBreakerRegistrationReport
{
    public DateTime Date { get; set; }
    public string? Organization { get; set; }
    public string? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? PipelineId { get; set; }
    public string? PipelineName { get; set; }
    public string? PipelineType { get; set; }
    public string? PipelineVersion { get; set; }
    public string? RunId { get; set; }
    public string? RunUrl { get; set; }
    public string? StageId { get; set; }

    [Display(Name = "RegistrationStatus_s")]
    public string? RegistrationStatus { get; set; }

    public string? CiName { get; set; }
    public string? CiIdentifier { get; set; }

    [Display(Name = "Result_d")]
    public PipelineBreakerResult Result { get; set; }
}
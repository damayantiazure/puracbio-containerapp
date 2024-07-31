#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public class PipelineBreakerLogDto: LogModelDtoBase
{
    [Display(Name = "Date_t")]
    public DateTime Date { get; set; }

    [Display(Name = "Organization_s")]
    public string? Organization { get; set; }

    [Display(Name = "ProjectId_g")]
    public string? ProjectId { get; set; }

    [Display(Name = "ProjectName_s")]
    public string? ProjectName { get; set; }

    [Display(Name = "PipelineId_s")]
    public string? PipelineId { get; set; }

    [Display(Name = "PipelineName_s")]
    public string? PipelineName { get; set; }

    [Display(Name = "PipelineType_s")]
    public string? PipelineType { get; set; }

    [Display(Name = "PipelineVersion_s")]
    public string? PipelineVersion { get; set; }

    [Display(Name = "RunId_s")]
    public string? RunId { get; set; }

    [Display(Name = "RunUrl_s")]
    public string? RunUrl { get; set; }

    [Display(Name = "StageId_s")]
    public string? StageId { get; set; }

    [Display(Name = "RegistrationStatus_s")]
    public string? RegistrationStatus { get; set; }

    [Display(Name = "CiName_s")]
    public string? CiName { get; set; }

    [Display(Name = "CiIdentifier_s")]
    public string? CiIdentifier { get; set; }

    [Display(Name = "Result_d")]
    public int Result { get; set; }
}
#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public class DecoratorErrorLogDto : LogModelDtoBase
{
    [Display(Name = "ReleaseId_s")]
    public string? ReleaseId { get; set; }

    [Display(Name = "PipelineType_s")]
    public string? PipelineType { get; set; }

    [Display(Name = "StageName_s")]
    public string? StageName { get; set; }

    [Display(Name = "Organization_s")]
    public string? Organization { get; set; }

    [Display(Name = "ProjectId_g")]
    public string? ProjectId { get; set; }

    [Display(Name = "RunId_s")]
    public string? RunId { get; set; }

    [Display(Name = "Message")]
    public string? Message { get; set; }
}
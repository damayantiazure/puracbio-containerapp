#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public class Sm9ChangesErrorLogDto: LogModelExceptionDtoBase
{
    [Display(Name = "Request_s")]
    public string? Request { get; set; }

    [Display(Name = "RequestUrl_s")]
    public string? RequestUrl { get; set; }

    [Display(Name = "ProjectId_g")]
    public string? ProjectId { get; set; }

    [Display(Name = "Organization_s")]
    public string? Organization { get; set; }

    [Display(Name = "PipelineType_s")]
    public string? PipelineType { get; set; }

    [Display(Name = "RunId_s")]
    public string? RunId { get; set; }
}
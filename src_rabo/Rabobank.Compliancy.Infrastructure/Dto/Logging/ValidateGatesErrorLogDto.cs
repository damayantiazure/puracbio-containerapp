#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public class ValidateGatesErrorLogDto: LogModelExceptionDtoBase
{
    [Display(Name = "RequestUrl_s")]
    public string? RequestUrl { get; set; }

    [Display(Name = "ReleaseId_s")]
    public string? ReleaseId { get; set; }

    [Display(Name = "Request_s")]
    public string? Request { get; set; }

    [Display(Name = "ProjectId_s")]
    public string? ProjectId { get; set; }

    [Display(Name = "RunId_s")]
    public string? RunId { get; set; }

    [Display(Name = "StageId_s")]
    public string? StageId { get; set; }
}
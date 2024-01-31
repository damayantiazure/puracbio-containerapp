#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public class PipelineBreakerErrorLogDto : LogModelExceptionDtoBase
{
    [Display(Name = "RunId_s")]
    public string? RunId { get; set; }

    [Display(Name = "Request_s")]
    public string? Request { get; set; }

    [Display(Name = "ItemType_s")]
    public string? ItemType { get; set; }

    [Display(Name = "ProjectId_g")]
    public string? ProjectId { get; set; }

    [Display(Name = "RequestUrl_s")]
    public string? RequestUrl { get; set; }

    [Display(Name = "Organization_s")]
    public string? Organization { get; set; }

    [Display(Name = "ItemId_s")]
    public string? ItemId { get; set; }

    [Display(Name = "RuleName_s")]
    public string? RuleName { get; set; }
}
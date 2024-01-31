#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public class AuditLoggingErrorLogDto: LogModelExceptionDtoBase
{
    [Display(Name = "EventQueueData_s")] 
    public string? EventQueueData { get; set; } 

    [Display(Name = "RunUrl_s")]
    public string? RunUrl { get; set; } 

    [Display(Name = "RequestData_s")]
    public string? RequestData { get; set; } 

    [Display(Name = "ReleaseUrl_s")]
    public string? ReleaseUrl { get; set; } 

    [Display(Name = "Organization_s")]
    public string? Organization { get; set; } 

    [Display(Name = "ProjectId_g")]
    public string? ProjectId { get; set; }

    [Display(Name = "PullRequestUrl_s")]
    public string? PullRequestUrl { get; set; }
}
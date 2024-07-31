#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public class AuditLoggingHookFailureLogDto: LogModelDtoBase
{
    [Display(Name = "ErrorMessage_s")]
    public string? ErrorMessage { get; set; } 

    [Display(Name = "ErrorDetail_s")]
    public string? ErrorDetail { get; set; } 

    [Display(Name = "Organization_s")]
    public string? Organization { get; set; } 

    [Display(Name = "ProjectId_g")]
    public Guid? ProjectId { get; set; }

    [Display(Name = "PipelineId_s")] 
    public string? PipelineId { get; set; } 

    [Display(Name = "HookId_g")]
    public string? HookId { get; set; }

    [Display(Name = "EventId_g")]
    public string? EventId { get; set; }

    [Display(Name = "EventType_s")]
    public string? EventType { get; set; } 

    [Display(Name = "EventResourceData_s")]
    public string? EventResourceData { get; set; }

    [Display(Name = "Date_t")]
    public DateTime Date { get; set; }
}
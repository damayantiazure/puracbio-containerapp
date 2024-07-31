#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public class AuditPoisonMessagesLogDto: LogModelDtoBase
{
    [Display(Name = "FailedQueueTrigger_s")]
    public string? FailedQueueTrigger { get; set; } 

    [Display(Name = "MessageText_s")]
    public string? MessageText { get; set; } 
}
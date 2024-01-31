#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public abstract class LogModelExceptionDtoBase: LogModelDtoBase
{
    [Display(Name = "CorrelationId")]
    public string? CorrelationId { get; set; } 

    [Display(Name = "ExceptionType_s")]
    public string? ExceptionType { get; set; } 

    [Display(Name = "ExceptionMessage_s")]
    public string? ExceptionMessage { get; set; } 

    [Display(Name = "FunctionName_s")]
    public string? FunctionName { get; set; } 

    [Display(Name = "InnerExceptionMessage_s")]
    public string? InnerExceptionMessage { get; set; } 

    [Display(Name = "InnerExceptionType_s")]
    public string? InnerExceptionType { get; set; } 

    [Display(Name = "Date_t")]
    public DateTime Date { get; set; }
}
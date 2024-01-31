#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public class ErrorHandlingLogDto : LogModelExceptionDtoBase
{
    [Display(Name = "Organization_s")]
    public string? Organization { get; set; }

    [Display(Name = "ScanDate_t")]
    public DateTime? ScanDate { get; set; }

    [Display(Name = "ProjectId_g")]
    public string? ProjectId { get; set; }

    [Display(Name = "ProjectName_s")]
    public string? ProjectName { get; set; }
}
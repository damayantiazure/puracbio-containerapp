#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public abstract class LogModelDtoBase
{
    [Display(Name = "TimeGenerated")]
    public DateTime TimeGenerated { get; set; }
}
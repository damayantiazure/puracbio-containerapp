#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public class RuleCompliancyLogDto
{
    [Display(Name = "RuleDescription")]
    public string? RuleDescription { get; set; }

    [Display(Name = "IsCompliant")]
    public bool IsCompliant { get; set; }

    [Display(Name = "HasDeviation")]
    public bool HasDeviation { get; set; }

    [Display(Name = "ItemName")]
    public string? ItemName { get; set; }
}
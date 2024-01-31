#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public class CompliancyPrinciplesDto: LogModelDtoBase
{
    [Display(Name = "organization_s")]
    public string? Organization { get; set; }

    [Display(Name = "hasDeviation_b")]
    public bool HasDeviation { get; set; }

    [Display(Name = "projectId_g")]
    public string? ProjectId { get; set; }

    [Display(Name = "projectName_s")]
    public string? ProjectName { get; set; }

    [Display(Name = "ciId_s")]
    public string? CiId { get; set; }

    [Display(Name = "ciName_s")]
    public string? CiName { get; set; }

    [Display(Name = "isCompliant_b")]
    public bool IsCompliant { get; set; }

    [Display(Name = "scanDate_t")]
    public DateTime? ScanDate { get; set; }

    [Display(Name = "principleName_s")]
    public string? Name { get; set; }
}
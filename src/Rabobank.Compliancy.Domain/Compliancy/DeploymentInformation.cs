#nullable enable

using System.ComponentModel.DataAnnotations;

namespace Rabobank.Compliancy.Domain.Compliancy;

public class DeploymentInformation
{
    [Display(Name = "CompletedOn_t")]
    public DateTime? CompletedOn { get; set; }

    [Display(Name = "CiName_s")]
    public string? CiName { get; set; }

    [Display(Name = "RunUrl_s")]
    public string? RunUrl { get; set; }
}
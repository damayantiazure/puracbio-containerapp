#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Dto.Logging;

[ExcludeFromCodeCoverage]
public class AuditDeploymentLogDto : LogModelDtoBase
{
    [Display(Name = "CompletedOn_t")]
    public DateTime? CompletedOn { get; set; }

    [Display(Name = "CiName_s")]
    public string? CiName { get; set; }

    [Display(Name = "RunUrl_s")]
    public string? RunUrl { get; set; }

    [Display(Name = "FortifyRan_b")]
    public bool FortifyRan { get; set; }

    [Display(Name = "SonarRan_b")]
    public bool SonarRan { get; set; }

    [Display(Name = "BuildUrls_s")]
    public IEnumerable<string>? BuildUrls { get; set; }

    [Display(Name = "RepoUrls_s")]
    public IEnumerable<string>? RepoUrls { get; set; }

    [Display(Name = "ProjectId_g")]
    public string? ProjectId { get; set; }

    [Display(Name = "Organization_s")]
    public string? Organization { get; set; }

    [Display(Name = "SM9ChangeUrl_s")]
    public string? Sm9ChangeUrl { get; set; }

    [Display(Name = "ProjectName_s")]
    public string? ProjectName { get; set; }

    [Display(Name = "StageId_s")]
    public string? StageId { get; set; }

    [Display(Name = "SM9ChangeId_s")]
    public string? Sm9ChangeId { get; set; }

    [Display(Name = "PipelineName_s")]
    public string? PipelineName { get; set; }

    [Display(Name = "RunName_s")]
    public string? RunName { get; set; }

    [Display(Name = "RunId_s")]
    public string? RunId { get; set; }

    [Display(Name = "PipelineId_s")]
    public string? PipelineId { get; set; }

    [Display(Name = "StageName_s")]
    public string? StageName { get; set; }

    [Display(Name = "PipelineApproval_b")]
    public bool PipelineApproval { get; set; }

    [Display(Name = "PullRequestApproval_b")]
    public bool PullRequestApproval { get; set; }

    [Display(Name = "ArtifactIntegrity_b")]
    public bool ArtifactIntegrity { get; set; }

    [Display(Name = "DeploymentStatus_s")]
    public string? DeploymentStatus { get; set; }

    [Display(Name = "CiIdentifier_s")]
    public string? CiIdentifier { get; set; }

    [Display(Name = "IsSox_b")]
    public bool IsSox { get; set; }
}
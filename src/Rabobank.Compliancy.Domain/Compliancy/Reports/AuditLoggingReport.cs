#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class AuditLoggingReport
{
    public string? Organization { get; set; }
    public string? ProjectName { get; set; }
    public string? ProjectId { get; set; }
    public string? PipelineName { get; set; }
    public string? PipelineId { get; set; }
    public string? StageName { get; set; }
    public string? StageId { get; set; }
    public string? RunName { get; set; }
    public string? RunId { get; set; }
    public string? RunUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime CompletedOn { get; set; }
    public bool PipelineApproval { get; set; }
    public bool PullRequestApproval { get; set; }
    public bool ArtifactIntegrity { get; set; }

    [Display(Name="SM9ChangeId_s")]
    public string? Sm9ChangeId { get; set; }

    [Display(Name = "SM9ChangeUrl_s")]
    public Uri? Sm9ChangeUrl { get; set; }
    public string? DeploymentStatus { get; set; }
    public bool IsSox { get; set; }
    public string? CiIdentifier { get; set; }
    public string? CiName { get; set; }
    public IEnumerable<Uri>? BuildUrls { get; set; }
    public IEnumerable<Uri>? RepoUrls { get; set; }
    public bool SonarRan { get; set; }
    public bool FortifyRan { get; set; }
}
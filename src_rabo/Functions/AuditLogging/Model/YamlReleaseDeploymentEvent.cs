using System;

namespace Rabobank.Compliancy.Functions.AuditLogging.Model;

public class YamlReleaseDeploymentEvent
{
    public string Organization { get; set; }
    public string ProjectId { get; set; }
    public string PipelineName { get; set; }
    public string PipelineId { get; set; }
    public string RunName { get; set; }
    public string RunId { get; set; }
    public string StageName { get; set; }
    public string StageId { get; set; }
    public string RunUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public string DeploymentStatus { get; set; }
}
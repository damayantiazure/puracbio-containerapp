using System;

namespace Rabobank.Compliancy.Functions.AuditLogging.Model;

public class ClassicReleaseDeploymentEvent
{
    public string Organization { get; set; }
    public string ProjectName { get; set; }
    public string ProjectId { get; set; }
    public string StageName { get; set; }
    public string ReleaseId { get; set; }
    public string ReleaseUrl { get; set; }
    public DateTime CreatedDate { get; set; }
}
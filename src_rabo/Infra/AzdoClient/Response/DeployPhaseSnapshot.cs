using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class DeployPhaseSnapshot
{
    public DeploymentInput DeploymentInput { get; set; }
    public string PhaseType { get; set; }
    public IEnumerable<WorkflowTask> WorkflowTasks { get; set; }
}
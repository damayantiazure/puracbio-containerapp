using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class DeployPhase
{
    public IList<WorkflowTask> WorkflowTasks { get; set; }
}
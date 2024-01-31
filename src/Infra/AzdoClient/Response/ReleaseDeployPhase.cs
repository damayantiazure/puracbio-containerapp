using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class ReleaseDeployPhase
{
    public int Id { get; set; }
    public IEnumerable<DeploymentJob> DeploymentJobs { get; set; }
}
using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class ReleaseDefinitionEnvironment
{
    public int Id { get; set; }
    public string Name { get; set; }
    public IList<DeployPhase> DeployPhases { get; set; }
    public RetentionPolicy RetentionPolicy { get; set; }
    public PreDeployApprovals PreDeployApprovals { get; set; }
    public ReleaseDefinitionGatesStep PreDeploymentGates { get; set; }
    public IList<Condition> Conditions { get; set; }
}
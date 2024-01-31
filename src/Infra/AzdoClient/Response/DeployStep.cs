using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class DeployStep
{
    public IdentityRef RequestedFor { get; set; }
    public IdentityRef LastModifiedBy { get; set; }
    public IEnumerable<ReleaseDeployPhase> ReleaseDeployPhases { get; set; }
}
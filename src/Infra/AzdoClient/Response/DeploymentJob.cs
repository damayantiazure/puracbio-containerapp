using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class DeploymentJob
{
    public IEnumerable<Task> Tasks { get; set; }
}
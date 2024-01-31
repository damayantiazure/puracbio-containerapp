using Rabobank.Compliancy.Functions.AuditLogging.Model;
using System.Linq;

namespace Rabobank.Compliancy.Functions.AuditLogging.Helpers;

public static class EventExtensions
{
    private static readonly string[] UnexecutedDeploymentStatuses =
        { "canceled", "skipped", "notDeployed" };

    public static bool IsDeploymentExecuted(this YamlReleaseDeploymentEvent evt) =>
        evt != null && IsExecuted(evt.DeploymentStatus);

    private static bool IsExecuted(string deploymentStatus) => 
        !UnexecutedDeploymentStatuses.Contains(deploymentStatus);
}
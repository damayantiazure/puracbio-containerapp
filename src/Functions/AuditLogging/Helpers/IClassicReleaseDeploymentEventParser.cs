using Rabobank.Compliancy.Functions.AuditLogging.Model;

namespace Rabobank.Compliancy.Functions.AuditLogging.Helpers;

public interface IClassicReleaseDeploymentEventParser
{
    ClassicReleaseDeploymentEvent Parse(string json);
}
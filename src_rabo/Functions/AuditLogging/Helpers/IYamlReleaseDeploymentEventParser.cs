using Rabobank.Compliancy.Functions.AuditLogging.Model;

namespace Rabobank.Compliancy.Functions.AuditLogging.Helpers;

public interface IYamlReleaseDeploymentEventParser
{
    YamlReleaseDeploymentEvent Parse(string json);
}
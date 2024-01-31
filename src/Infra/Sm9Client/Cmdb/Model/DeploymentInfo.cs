using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

[ExcludeFromCodeCoverage]
public class DeploymentInfo
{
    public string? DeploymentMethod { get; set; }

    public string? SupplementaryInformation { get; set; }
}
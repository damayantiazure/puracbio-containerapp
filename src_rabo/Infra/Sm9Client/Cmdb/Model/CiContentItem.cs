using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

[ExcludeFromCodeCoverage]
public class CiContentItem
{
    public ConfigurationItemModel? Device { get; set; }

    public bool IsProduction =>
        Device != null &&
        Device.Status != null && Device.Status.StartsWith("In Use - ", StringComparison.CurrentCulture) &&
        Device.Environment != null && Device.Environment.Contains("Production");

}
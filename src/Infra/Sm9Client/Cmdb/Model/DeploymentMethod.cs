using Rabobank.Compliancy.Domain.RuleProfiles;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

[ExcludeFromCodeCoverage]
public class DeploymentMethod
{
    public DeploymentMethod(string? ciName, string organization,
        string project, string pipeline, string stage, string? profile)
    {
        CiName = ciName ?? throw new ArgumentNullException(nameof(ciName));
        Organization = organization ?? throw new ArgumentNullException(nameof(organization));
        Project = project ?? throw new ArgumentNullException(nameof(project));
        Pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        Stage = stage ?? throw new ArgumentNullException(nameof(stage));
        Profile = RuleProfile.GetValidProfileName(profile);
    }

    public DeploymentMethod(ConfigurationItem configurationItem, SupplementaryInformation existingMethod)
        : this(
            configurationItem.CiName,
            existingMethod.Organization,
            existingMethod.Project,
            existingMethod.Pipeline,
            existingMethod.Stage,
            existingMethod.Profile)
    {
    }

    public string CiName { get; set; }

    public string Organization { get; set; }

    public string Project { get; set; }

    public string Pipeline { get; set; }

    public string Stage { get; set; }

    public Profiles Profile { get; set; }

    /// <summary>
    /// Returns the string in the format used in ITMS/SM9
    /// </summary>
    /// <returns></returns>
    public override string ToString() => 
        $"{{\"organization\":\"{Organization}\",\"project\":\"{Project}\",\"pipeline\":\"{Pipeline}\",\"stage\":\"{Stage}\", \"profile\":\"{Profile}\"}}";
}
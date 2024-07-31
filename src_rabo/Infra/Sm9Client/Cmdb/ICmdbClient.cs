using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb;

public interface ICmdbClient
{
    Task<ConfigurationItem?> GetCiAsync(string ciIdentifier);
    Task<AssignmentGroup?> GetAssignmentGroupAsync(string assignmentGroupName);
    Task<IEnumerable<DeploymentInformation>?> GetDeploymentMethodAsync(string? ciName);

    /// <summary>
    ///     Insert a new Deployment Method for a given <see cref="ConfigurationItem" />
    /// </summary>
    /// <param name="newMethod">
    ///     The details of the new Method, including the Name of the <see cref="ConfigurationItem" />, to
    ///     be added in Cmdb
    /// </param>
    /// <returns></returns>
    Task<ManageDeploymentInformationResponse?> InsertDeploymentMethodAsync(DeploymentMethod? newMethod);

    /// <summary>
    ///     Updates an existing Deployment Method
    /// </summary>
    /// <param name="configurationItem"><see cref="ConfigurationItem" /> of which a method needs to be updated</param>
    /// <param name="currentMethod">
    ///     The existing record in the Cmdb for the <see cref="ConfigurationItem" /> that needs to be
    ///     updated
    /// </param>
    /// <param name="newMethod">The new value with which the current value needs to be replaced</param>
    /// <returns></returns>
    Task<ManageDeploymentInformationResponse?> UpdateDeploymentMethodAsync(ConfigurationItem? configurationItem,
        SupplementaryInformation? currentMethod, DeploymentMethod? newMethod);

    /// <summary>
    ///     Delete an existing Deployment Method
    /// </summary>
    /// <param name="configurationItem"><see cref="ConfigurationItem" /> of which a method needs to be deleted</param>
    /// <param name="methodToDelete">The existing record in the Cmdb for the <see cref="ConfigurationItem" /> that needs to be deleted</param>
    /// <returns></returns>
    Task<ManageDeploymentInformationResponse?> DeleteDeploymentMethodAsync(ConfigurationItem configurationItem,
        SupplementaryInformation methodToDelete);

    Task<IEnumerable<CiContentItem>> GetAzDoCIsLegacyAsync();

    Task<IEnumerable<CiContentItem>> GetAzDoCIsAsync();
}
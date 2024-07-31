using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Models;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

public interface IExtensionDataRepository
{
    /// <summary>
    ///     Upload extension data to the specified Azure DevOps extension.
    /// </summary>
    /// <param name="publisher">Publisher of the extension.</param>
    /// <param name="collection">Name of the collection.</param>
    /// <param name="extensionName">Name of the extension</param>
    /// <param name="organization">The organization</param>
    /// <param name="extensionData">The data to be uploaded.</param>
    /// <param name="cancellationToken">The cancellationToken</param>
    /// <returns>The uploaded extension data or null.</returns>
    public Task<TExtensionData?> UploadAsync<TExtensionData>(
        string publisher, string collection, string extensionName, string organization,
        TExtensionData extensionData, CancellationToken cancellationToken = default)
        where TExtensionData : ExtensionData;

    /// <summary>
    ///     Upload extension data to the specified Azure DevOps extension by identifier.
    /// </summary>
    /// <param name="publisher">Publisher of the extension.</param>
    /// <param name="collection">Name of the collection.</param>
    /// <param name="extensionName">Name of the extension</param>
    /// <param name="organization">The organization</param>
    /// <param name="id">The identifier of the extension data.</param>
    /// <param name="cancellationToken">The cancellationToken</param>
    /// <returns>An object representing the downloaded extension data or null.</returns>
    public Task<TExtensionData?> DownloadAsync<TExtensionData>(
        string publisher, string collection, string extensionName, string organization,
        string id, CancellationToken cancellationToken = default)
        where TExtensionData : ExtensionData;
}
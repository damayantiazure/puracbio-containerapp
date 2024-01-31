using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.ExtensionManagement;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Models;
using Rabobank.Compliancy.Clients.HttpClientExtensions;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

public class ExtensionDataRepository : IExtensionDataRepository
{
    private readonly IExtmgmtHttpClientCallHandler _httpClientCallHandler;

    public ExtensionDataRepository(
        IExtmgmtHttpClientCallHandler httpClientCallHandler) =>
        _httpClientCallHandler = httpClientCallHandler;

    /// <inheritdoc />
    public Task<TExtensionData?> UploadAsync<TExtensionData>(
        string publisher, string collection, string extensionName, string organization,
        TExtensionData extensionData, CancellationToken cancellationToken = default)
        where TExtensionData : ExtensionData
    {
        var request = new UploadExtensionDataRequest<TExtensionData>(
            publisher, collection, extensionName, organization, extensionData, _httpClientCallHandler);

        return request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<TExtensionData?> DownloadAsync<TExtensionData>(
        string publisher, string collection, string extensionName, string organization,
        string id, CancellationToken cancellationToken = default)
        where TExtensionData : ExtensionData
    {
        var request = new DownloadExtensionDataRequest<TExtensionData>(
            publisher, collection, extensionName, organization, id, _httpClientCallHandler);

        return request.ExecuteAsync(cancellationToken: cancellationToken);
    }
}
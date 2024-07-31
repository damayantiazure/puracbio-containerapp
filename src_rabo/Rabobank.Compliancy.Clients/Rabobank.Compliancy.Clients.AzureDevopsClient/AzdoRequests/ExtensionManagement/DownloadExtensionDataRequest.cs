using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.ExtensionManagement;

public class DownloadExtensionDataRequest<TExtensionData>
    : HttpGetRequest<IExtmgmtHttpClientCallHandler, TExtensionData>
    where TExtensionData : Repositories.Models.ExtensionData
{
    private readonly string _collection;
    private readonly string _extensionName;
    private readonly string _id;
    private readonly string _organization;
    private readonly string _publisher;

    public DownloadExtensionDataRequest(
        string publisher, string collection, string extensionName, string organization, string id,
        IExtmgmtHttpClientCallHandler callHandler)
        : base(callHandler)
    {
        _publisher = publisher;
        _collection = collection;
        _extensionName = extensionName;
        _organization = organization;
        _id = id;
    }

    protected override string Url =>
        $"{_organization}/_apis/ExtensionManagement/InstalledExtensions/{_publisher}/{_extensionName}" +
        $"/Data/Scopes/Default/Current/Collections/{_collection}/Documents/{_id}";

    protected override Dictionary<string, string> QueryStringParameters { get; } = new()
    {
        { "api-version", "6.1-preview" }
    };
}
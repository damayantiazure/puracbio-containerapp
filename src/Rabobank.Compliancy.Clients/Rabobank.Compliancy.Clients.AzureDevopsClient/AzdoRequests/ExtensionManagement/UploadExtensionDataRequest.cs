using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.ExtensionManagement;

public class UploadExtensionDataRequest<TExtensionData> : HttpPutRequest<IExtmgmtHttpClientCallHandler,
    TExtensionData, TExtensionData> where TExtensionData : Repositories.Models.ExtensionData
{
    private readonly string _collection;
    private readonly string _extensionName;
    private readonly string _organization;
    private readonly string _publisher;

    public UploadExtensionDataRequest(
        string publisher, string collection,
        string extensionName, string organization,
        TExtensionData value,
        IExtmgmtHttpClientCallHandler callHandler) : base(value, callHandler)
    {
        _publisher = publisher;
        _collection = collection;
        _extensionName = extensionName;
        _organization = organization;
    }

    protected override string Url =>
        $"{_organization}/_apis/ExtensionManagement/InstalledExtensions/{_publisher}/{_extensionName}" +
        $"/Data/Scopes/Default/Current/Collections/{_collection}/Documents";

    protected override Dictionary<string, string> QueryStringParameters { get; } = new()
    {
        { "api-version", "6.1-preview" }
    };
}
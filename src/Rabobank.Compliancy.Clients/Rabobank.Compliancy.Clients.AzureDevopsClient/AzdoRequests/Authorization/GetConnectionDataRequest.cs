using Microsoft.VisualStudio.Services.Location;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Authorization;

internal class GetConnectionDataRequest : HttpGetRequest<IDevHttpClientCallHandler, ConnectionData>
{
    private readonly string _organization;

    protected override string Url => $"{_organization}/_apis/connectionData";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "5.0-preview" }
    };

    public GetConnectionDataRequest(string organization, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
    }
}
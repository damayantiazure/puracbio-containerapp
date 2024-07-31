using Microsoft.VisualStudio.Services.ServiceHooks.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Hooks;

/// <summary>
/// Used to get a list of subscriptions "{_organization}/_apis/hooks/subscriptions".
/// </summary>
public class GetSubscriptionsRequest : HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<Subscription>>
{
    private readonly string _organization;

    protected override string Url => $"{_organization}/_apis/hooks/subscriptions";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.1-preview" }
    };

    public GetSubscriptionsRequest(string organization, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
    }
}
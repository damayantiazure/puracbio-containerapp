using Microsoft.VisualStudio.Services.ServiceHooks.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Hooks;

/// <summary>
/// Used to get a specific service hooks subscription "{_organization}/_apis/hooks/subscriptions/{_subscriptionId}".
/// </summary>
public class GetSubscriptionRequest : HttpGetRequest<IDevHttpClientCallHandler, Subscription>
{
    private readonly string _organization;
    private readonly string _subscriptionId;

    protected override string Url => $"{_organization}/_apis/hooks/subscriptions/{_subscriptionId}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.1-preview" }
    };

    public GetSubscriptionRequest(string organization, Guid subscriptionId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _subscriptionId = subscriptionId.ToString();
    }
}
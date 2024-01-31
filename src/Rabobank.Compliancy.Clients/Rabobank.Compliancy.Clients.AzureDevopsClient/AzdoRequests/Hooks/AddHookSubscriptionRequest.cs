using Microsoft.VisualStudio.Services.ServiceHooks.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Hooks;

/// <summary>
/// Used to create a subscription "{_organization}/_apis/hooks/subscriptions".
/// </summary>
public class AddHookSubscriptionRequest : HttpPostRequest<IDevHttpClientCallHandler, Subscription, Subscription>
{
    private readonly string _organization;

    protected override string Url => $"{_organization}/_apis/hooks/subscriptions";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.1-preview" }
    };

    public AddHookSubscriptionRequest(string organization, Subscription subscription, IDevHttpClientCallHandler httpClientCallHandler)
        : base(subscription, httpClientCallHandler)
    {
        _organization = organization;
    }
}
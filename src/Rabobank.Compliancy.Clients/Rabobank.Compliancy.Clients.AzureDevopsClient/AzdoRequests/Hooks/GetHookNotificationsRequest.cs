using Microsoft.VisualStudio.Services.ServiceHooks.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Hooks;

/// <summary>
/// Used to get a list of notifications for a specific subscription "{_organization}/_apis/hooks/subscriptions/{_subscriptionId}/notifications".
/// </summary>
public class GetHookNotificationsRequest : HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<Notification>>
{
    private readonly string _organization;
    private readonly string _subscriptionId;

    protected override string Url => $"{_organization}/_apis/hooks/subscriptions/{_subscriptionId}/notifications";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.1-preview" }
    };

    public GetHookNotificationsRequest(string organization, Guid subscriptionId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _subscriptionId = subscriptionId.ToString();
    }
}
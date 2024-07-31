using Microsoft.VisualStudio.Services.ServiceHooks.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Hooks;

/// <summary>
/// Used to get a specific notification for a subscription "{_organization}/_apis/hooks/subscriptions/{_subscriptionId}/notifications/{_notificationId}".
/// </summary>
public class GetHookNotificationRequest : HttpGetRequest<IDevHttpClientCallHandler, Notification>
{
    private readonly string _organization;
    private readonly string _subscriptionId;
    private readonly string _notificationId;

    protected override string Url => $"{_organization}/_apis/hooks/subscriptions/{_subscriptionId}/notifications/{_notificationId}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.1-preview" }
    };

    public GetHookNotificationRequest(string organization, Guid subscriptionId, int notificationId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _subscriptionId = subscriptionId.ToString();
        _notificationId = notificationId.ToString();
    }
}
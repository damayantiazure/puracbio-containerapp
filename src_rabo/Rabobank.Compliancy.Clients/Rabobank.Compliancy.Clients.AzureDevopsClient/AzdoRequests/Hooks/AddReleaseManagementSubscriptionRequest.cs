using Microsoft.VisualStudio.Services.ServiceHooks.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Hooks.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Hooks;

/// <summary>
/// Used to add a release management subscription "_apis/hooks/subscriptions".
/// </summary>
public class AddReleaseManagementSubscriptionRequest : HttpPostRequest<IVsrmHttpClientCallHandler, SubscriptionsQuery, ReleaseManagementSubscriptionBodyContent>
{
    private readonly string _organization;

    protected override string Url => $"{_organization}/_apis/hooks/subscriptions";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        {"api-version", "6.0"}
    };

    public AddReleaseManagementSubscriptionRequest(string organization, ReleaseManagementSubscriptionBodyContent releaseManagement, IVsrmHttpClientCallHandler httpClientCallHandler)
        : base(releaseManagement, httpClientCallHandler)
    {
        _organization = organization;
    }
}
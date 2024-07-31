using Microsoft.VisualStudio.Services.ServiceHooks.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Hooks;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Hooks.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

public class HooksRepository : IHooksRepository
{
    private readonly IDevHttpClientCallHandler _httpClientCallHandler;
    private readonly IVsrmHttpClientCallHandler _httpVsrmClientCallHandler;

    public HooksRepository(IDevHttpClientCallHandler httpClientCallHandler, IVsrmHttpClientCallHandler httpVsrmClientCallHandler)
    {
        _httpClientCallHandler = httpClientCallHandler;
        _httpVsrmClientCallHandler = httpVsrmClientCallHandler;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Subscription>?> GetSubscriptionsAsync(string organization, CancellationToken cancellationToken = default)
    {
        var request = new GetSubscriptionsRequest(organization, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc/>
    public async Task<Subscription?> GetSubscriptionAsync(string organization, Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var request = new GetSubscriptionRequest(organization, subscriptionId, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Notification?> GetHookNotificationAsync(string organization, Guid subscriptionId, int notificationId, CancellationToken cancellationToken = default)
    {
        var request = new GetHookNotificationRequest(organization, subscriptionId, notificationId, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Notification>?> GetHookNotificationsAsync(string organization, Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var request = new GetHookNotificationsRequest(organization, subscriptionId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc/>
    public async Task<Subscription?> AddHookSubscriptionAsync(string organization, Subscription subscription, CancellationToken cancellationToken = default)
    {
        var request = new AddHookSubscriptionRequest(organization, subscription, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<SubscriptionsQuery?> AddReleaseManagementSubscriptionAsync(string organization, ReleaseManagementSubscriptionBodyContent releaseManagement, CancellationToken cancellationToken = default)
    {
        var request = new AddReleaseManagementSubscriptionRequest(organization, releaseManagement, _httpVsrmClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }
}
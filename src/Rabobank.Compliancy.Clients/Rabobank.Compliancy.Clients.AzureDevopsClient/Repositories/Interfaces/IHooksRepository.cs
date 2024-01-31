using Microsoft.VisualStudio.Services.ServiceHooks.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Hooks.Models;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

/// <summary>
/// Provides methods to cater to all object needs from the Azure Devops API regarding <see cref="HooksRepository"/>
/// </summary>
public interface IHooksRepository
{
    /// <summary>
    /// Get a list of subscriptions.
    /// </summary>
    /// <param name="organization">The organization the subscription belongs to</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable enumerable of <see cref="Subscription"/> representing a Subscription the way Azure Devops API returns it.</returns>
    Task<IEnumerable<Subscription>?> GetSubscriptionsAsync(string organization, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific service hooks subscription.
    /// </summary>
    /// <param name="organization">The organization the subscription belongs to</param>
    /// <param name="subscriptionId">The ID of the specific service hooks subscription</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable <see cref="Subscription"/> representing a Subscription the way Azure Devops API returns it.</returns>
    Task<Subscription?> GetSubscriptionAsync(string organization, Guid subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific notification for a subscription.
    /// </summary>
    /// <param name="organization">The organization the subscription belongs to</param>
    /// <param name="subscriptionId">The ID of the specific service hooks subscription</param>
    /// <param name="notificationId">The ID of the notification</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable <see cref="Notification"/> representing a Notification the way Azure Devops API returns it.</returns>
    Task<Notification?> GetHookNotificationAsync(string organization, Guid subscriptionId, int notificationId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Get a list of notifications for a specific subscription.
  /// </summary>
  /// <param name="organization">The organization the subscription belongs to</param>
  /// <param name="subscriptionId">The ID of the specific service hooks subscription</param>
  /// <param name="cancellationToken">Cancels the API call if necessary</param>
  /// <returns>Nullable enumerable of <see cref="Notification"/> representing a Subscription the way Azure Devops API returns it.</returns>
    Task<IEnumerable<Notification>?> GetHookNotificationsAsync(string organization, Guid subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a subscription.
    /// </summary>
    /// <param name="organization">The organization the subscription belongs to</param>
    /// <param name="subscription">Subscription the way Azure Devops API understands it</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable <see cref="Subscription"/> representing a Subscription the way Azure Devops API returns it.</returns>
    Task<Subscription?> AddHookSubscriptionAsync(string organization, Subscription subscription, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates release management subscription.
    /// </summary>
    /// <param name="organization">The organization the subscription belongs to</param>
    /// <param name="releaseManagement">Release management subscription to be created.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable <see cref="SubscriptionsQuery"/> representing a SubscriptionsQuery the way Azure Devops API returns it.</returns>
    Task<SubscriptionsQuery?> AddReleaseManagementSubscriptionAsync(string organization, ReleaseManagementSubscriptionBodyContent releaseManagement, CancellationToken cancellationToken = default);
}
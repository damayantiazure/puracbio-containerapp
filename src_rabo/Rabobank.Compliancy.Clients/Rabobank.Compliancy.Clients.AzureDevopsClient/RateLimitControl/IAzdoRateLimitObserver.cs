namespace Rabobank.Compliancy.Clients.AzureDevopsClient.RateLimitControl;


/// <summary>
/// Interface with operations to make is possible to keep track of the existance of rate limits for the 
/// used identities
/// </summary>
public interface IAzdoRateLimitObserver
{
    /// <summary>
    /// Get an available identity (identity without ratelimit set, except if all have rate limits)
    /// </summary>
    /// <returns>The available identity</returns>
    string GetAvailableIdentity();

    /// <summary>
    /// Sets a ratelimit delay until to a specific identity. This identity will be marked as unavailable.
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="rateLimitDelayUntil"></param>
    void SetRateLimitDelayForIdentity(string identity, DateTime rateLimitDelayUntil);

    /// <summary>
    /// Removes a ratelimit delay from a specific identity. This identity will be marked as available.
    /// </summary>
    /// <param name="identity"></param>
    void RemoveRateLimitDelayForIdentity(string identity);
}
using Microsoft.Extensions.Logging;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.RateLimitControl;

/// <inheritdoc/>
public class AzdoRateLimitObserver : IAzdoRateLimitObserver
{
    private readonly Dictionary<string, RateLimitInformation> _identitiesWithRateLimitInfo = new();
    private readonly ILogger<AzdoRateLimitObserver> _logger;
    private readonly string[] _identities;

    internal AzdoRateLimitObserver(string[] identities, ILogger<AzdoRateLimitObserver> logger)
    {
        if (identities == null || !identities.Any())
        {
            throw new InvalidOperationException("No identities available to set");
        }

        _identities = identities;

        foreach (var identity in identities)
        {
            _identitiesWithRateLimitInfo.Add(identity, new RateLimitInformation());
        }

        this._logger = logger;
    }

    /// <inheritdoc/>
    public void SetRateLimitDelayForIdentity(string identity, DateTime rateLimitDelayUntil)
    {
        var index = Array.FindIndex(_identities, i => i == identity);
        _logger.LogInformation($"Ratelimit is set for identity at index {index} to {rateLimitDelayUntil}");

        _identitiesWithRateLimitInfo[identity] = new RateLimitInformation(rateLimitDelayUntil);
    }

    /// <inheritdoc/>
    public void RemoveRateLimitDelayForIdentity(string identity)
    {
        var index = Array.FindIndex(_identities, i => i == identity);
        var rateLimitDelayUntil = _identitiesWithRateLimitInfo[identity].RateLimitDelayUntil;
        if (rateLimitDelayUntil != null && rateLimitDelayUntil <= DateTime.Now)
        {
            _logger.LogInformation($"Ratelimit is removed for identity at index {index}");
            _identitiesWithRateLimitInfo[identity] = new RateLimitInformation();
        }
    }

    /// <inheritdoc/>
    public string GetAvailableIdentity()
    {
        var indentiesWithoutRateLimits = _identitiesWithRateLimitInfo.Where(kv => kv.Value.IsAvailable).ToList();

        // No available identities left?
        if (!indentiesWithoutRateLimits.Any())
        {
            _logger.LogWarning("No identities available without ratelimits set.");

            // Just return the first one despite the fact it has rate limits.
            return _identitiesWithRateLimitInfo.First().Key;
        }

        var random = new Random();
        // Pick random identity from available identities (identities with no rate limit)
        return indentiesWithoutRateLimits[random.Next(0, indentiesWithoutRateLimits.Count)].Key;
    }
}
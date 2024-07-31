namespace Rabobank.Compliancy.Clients.AzureDevopsClient.RateLimitControl;

/// <summary>
/// internal class for holding information about ratelimits
/// </summary>
internal class RateLimitInformation
{
    public RateLimitInformation()
    {
    }

    public RateLimitInformation(DateTime rateLimitDelayUntil)
    {
        RateLimitDelayUntil = rateLimitDelayUntil;
    }

    public DateTime? RateLimitDelayUntil { get; private set; }
    public bool IsAvailable => RateLimitDelayUntil == null || RateLimitDelayUntil <= DateTime.UtcNow;
}

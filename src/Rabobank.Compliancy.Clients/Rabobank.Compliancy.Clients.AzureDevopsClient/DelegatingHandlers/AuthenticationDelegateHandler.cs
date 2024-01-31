using Rabobank.Compliancy.Clients.AzureDevopsClient.RateLimitControl;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.DelegatingHandlers;

/// <summary>
/// This class is responsible for setting the correct indentity, based on stored rate limit information
/// on the header when sending a http request.
/// If an authorization header is already present, this functionality is bypassed.
/// </summary>
public class AuthenticationDelegateHandler : DelegatingHandler
{
    private readonly IAzdoRateLimitObserver _rateLimitObserver;
    private readonly IIdentityProvider _identityProvider;

    /// <summary>
    /// Constructor which accepts a ratelimitObserver instance and an identity provider
    /// </summary>
    /// <param name="rateLimitObserver">An instance of a rateLimitObserver class responsible for ratelimit tracking of identities</param>
    /// <param name="identityProvider">A provider class responsible for the used identities</param>
    public AuthenticationDelegateHandler(IAzdoRateLimitObserver rateLimitObserver, IIdentityProvider identityProvider)
    {
        _rateLimitObserver = rateLimitObserver;
        _identityProvider = identityProvider;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // If an authentication header is already present, do not override it.
        // Some HTTP call require to set a custom token on the header of the call
        if (request.Headers.Authorization != null)
        {
            var responseMessage = await base.SendAsync(request, cancellationToken);
            return responseMessage;
        }

        var identityContextIdentifier = _rateLimitObserver.GetAvailableIdentity();
        var authenticationHeaderContext = _identityProvider.GetIdentityContext(identityContextIdentifier);
        request.Headers.Authorization = await authenticationHeaderContext.GetAuthenticationHeaderAsync(cancellationToken);

        var response = await base.SendAsync(request, cancellationToken);
        EvaluateRateLimitHeader(response, identityContextIdentifier);
        return response;
    }

    private void EvaluateRateLimitHeader(HttpResponseMessage response, string identity)
    {
        var epochSeconds = GetResponseHeaderLongValue(response, "X-RateLimit-Reset");

        // X-RateLimit-Reset header is not present so no rate-limit issues so far
        if (epochSeconds == null)
        {
            _rateLimitObserver.RemoveRateLimitDelayForIdentity(identity);
            return;
        }

        // If we reach this point, it means that the X-RateLimit-Reset header is present
        // and although we did not hit it yet, we will very soon so let's delay.

        // X-RateLimit-Reset contains a unix epoch describing the exact time when the
        // rate limit will be reset, usually 5 minutes. So if we wait until that time,
        // rate limits will be gone.
        var rateLimitDelayUntil = DateTimeOffset.FromUnixTimeSeconds(epochSeconds.Value).UtcDateTime;
        _rateLimitObserver.SetRateLimitDelayForIdentity(identity, rateLimitDelayUntil);
    }

    private static long? GetResponseHeaderLongValue(HttpResponseMessage response, string headerName)
    {
        if (response.Headers.TryGetValues(headerName, out var headerValues))
        {
            return long.Parse(headerValues.Single());
        }

        return null;
    }
}
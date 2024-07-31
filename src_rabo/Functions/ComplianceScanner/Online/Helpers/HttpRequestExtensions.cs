using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;

public static class HttpRequestExtensions
{
    /// <summary>
    /// Gets the authorization token from the <see cref="HttpRequest"/> header.
    /// </summary>
    /// <param name="httpRequest">The httprequest parameter.</param>
    /// <returns>The token as <see cref="string"/> or null when there is no authorization token.</returns>
    public static AuthenticationHeaderValue GetAuthorizationTokenOrDefault(this HttpRequest httpRequest)
        => AuthenticationHeaderValue.TryParse(httpRequest.Headers.Authorization, out var headerValue)
            ? headerValue
            : null;
}
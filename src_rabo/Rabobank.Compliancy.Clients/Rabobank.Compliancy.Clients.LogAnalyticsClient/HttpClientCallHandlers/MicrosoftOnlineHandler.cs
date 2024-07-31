using Rabobank.Compliancy.Clients.LogAnalyticsClient.HttpClientCallHandlers.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Clients.LogAnalyticsClient.HttpClientCallHandlers;

/// <inheritdoc/>
public class MicrosoftOnlineHandler : SpecificHttpClientCallHandlerBase, IMicrosoftOnlineHandler
{
    [SuppressMessage("Sonar Code Smell",
    "S1075: Refactor your code not to use hardcoded absolute paths or URIs.",
    Justification = "We will allow this since these URIs are unlikely to change.")]
    public const string SpecificBaseUrl = "https://login.microsoftonline.com/";

    protected override string BaseUrl => SpecificBaseUrl;

    public MicrosoftOnlineHandler(IHttpClientFactory httpClientFactory, string httpClientName)
        : base(httpClientFactory, httpClientName)
    {
    }
}
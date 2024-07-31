using Rabobank.Compliancy.Clients.LogAnalyticsClient.HttpClientCallHandlers.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Clients.LogAnalyticsClient.HttpClientCallHandlers;

/// <inheritdoc/>
public class LogAnalyticsCallHandler : SpecificHttpClientCallHandlerBase, ILogAnalyticsCallHandler
{
    [SuppressMessage("Sonar Code Smell",
        "S1075: Refactor your code not to use hardcoded absolute paths or URIs.",
        Justification = "We will allow this since these URIs are unlikely to change.")]
    public const string SpecificBaseUrl = "https://api.loganalytics.io";

    protected override string BaseUrl => SpecificBaseUrl;

    public LogAnalyticsCallHandler(IHttpClientFactory httpClientFactory, string httpClientName)
        : base(httpClientFactory, httpClientName)
    {
    }
}
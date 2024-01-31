using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions;

public class DevHttpClientCallHandler : SpecificHttpClientCallHandlerBase, IDevHttpClientCallHandler
{
    [SuppressMessage("Sonar Code Smell",
        "S1075: Refactor your code not to use hardcoded absolute paths or URIs.",
        Justification = "We will allow this since these URIs are unlikely to change.")]
    public const string SpecificBaseUrl = "https://dev.azure.com";
    protected override string BaseUrl => SpecificBaseUrl;

    public DevHttpClientCallHandler(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory, SpecificBaseUrl) { }
}
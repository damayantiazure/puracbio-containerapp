using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions;

public class VsrmHttpClientCallHandler : SpecificHttpClientCallHandlerBase, IVsrmHttpClientCallHandler
{
    [SuppressMessage("Sonar Code Smell",
        "S1075: Refactor your code not to use hardcoded absolute paths or URIs.",
        Justification = "We will allow this since these URIs are unlikely to change.")]
    public const string SpecificBaseUrl = "https://vsrm.dev.azure.com/";
    protected override string BaseUrl => SpecificBaseUrl;

    public VsrmHttpClientCallHandler(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory, SpecificBaseUrl) { }
}
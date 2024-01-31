using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions;

public class AuditserviceHttpClientCallHandler : SpecificHttpClientCallHandlerBase, IAuditserviceHttpClientCallHandler
{
    [SuppressMessage("Sonar Code Smell",
        "S1075: Refactor your code not to use hardcoded absolute paths or URIs.",
        Justification = "We will allow this since these URIs are unlikely to change.")]
    public const string SpecificBaseUrl = "https://auditservice.dev.azure.com/";
    protected override string BaseUrl => SpecificBaseUrl;

    public AuditserviceHttpClientCallHandler(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory, SpecificBaseUrl) { }
}
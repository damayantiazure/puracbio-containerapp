using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.IntegrationTests.ProjectInit.Interfaces;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.IntegrationTests.ProjectInit;

internal class ProjectInitHttpClientCallHandler : SpecificHttpClientCallHandlerBase, IProjectInitHttpClientCallHandler
{
    public static string SpecificBaseUrl = Environment.GetEnvironmentVariable("projectInitBaseUrl")
                                           ?? throw new InvalidOperationException($"No base url was specified for the http client handler {nameof(ProjectInitHttpClientCallHandler)}");

    protected override string BaseUrl => SpecificBaseUrl;

    public ProjectInitHttpClientCallHandler(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory, SpecificBaseUrl) { }
}
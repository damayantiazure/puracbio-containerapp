using Rabobank.Compliancy.Clients.HttpClientExtensions;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;

public abstract class SpecificHttpClientCallHandlerBase : HttpClientCallHandler
{
    protected abstract string BaseUrl { get; }

    protected SpecificHttpClientCallHandlerBase(IHttpClientFactory httpClientFactory, string httpClientName)
        : base(httpClientFactory, httpClientName) { }

    protected override HttpClient CreateAndCustomizeHttpClient(AuthenticationHeaderValue? authenticationHeaderValue)
    {
        var httpClient = ClientFactory.CreateClient(BaseUrl);

        AddCustomTokenToHttpClient(httpClient, authenticationHeaderValue);

        return httpClient;
    }
}
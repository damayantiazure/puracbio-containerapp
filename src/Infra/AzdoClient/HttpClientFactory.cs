using System.Net.Http;
using Flurl.Http.Configuration;
using Rabobank.Compliancy.Infra.AzdoClient.Handlers;

namespace Rabobank.Compliancy.Infra.AzdoClient;

public class HttpClientFactory : DefaultHttpClientFactory
{
    public override HttpClient CreateHttpClient(HttpMessageHandler handler)
    {
        return base.CreateHttpClient(new NotFoundHandler(handler));
    }
}
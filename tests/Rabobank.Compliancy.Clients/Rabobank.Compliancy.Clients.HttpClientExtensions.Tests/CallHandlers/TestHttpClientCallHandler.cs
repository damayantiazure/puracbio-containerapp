namespace Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.CallHandlers;

public class TestHttpClientCallHandler : HttpClientCallHandler, ITestHttpClientCallHandler
{
    public TestHttpClientCallHandler(IHttpClientFactory httpClientFactory, string httpClientName)
        : base(httpClientFactory, httpClientName) { }
}
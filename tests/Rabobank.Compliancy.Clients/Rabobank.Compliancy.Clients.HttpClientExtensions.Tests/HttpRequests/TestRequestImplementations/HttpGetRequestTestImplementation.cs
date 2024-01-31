using Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.CallHandlers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.HttpRequests.TestRequestImplementations;

public class HttpGetRequestTestImplementation : HttpGetRequest<ITestHttpClientCallHandler, object>
{
    public HttpGetRequestTestImplementation(ITestHttpClientCallHandler callHandler) : base(callHandler)
    {

    }

    protected override string? Url => "/test/test";

    protected override Dictionary<string, string> QueryStringParameters => new() { { "test", "parameter" } };
}
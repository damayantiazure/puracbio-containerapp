using Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.CallHandlers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.HttpRequests.TestRequestImplementations;

public class HttpGetRequestTestIncorrectImplementation : HttpGetRequest<ITestHttpClientCallHandler, object>
{
    public HttpGetRequestTestIncorrectImplementation(ITestHttpClientCallHandler callHandler) : base(callHandler)
    {

    }

    protected override string? Url => null;

    protected override Dictionary<string, string> QueryStringParameters => new();
}
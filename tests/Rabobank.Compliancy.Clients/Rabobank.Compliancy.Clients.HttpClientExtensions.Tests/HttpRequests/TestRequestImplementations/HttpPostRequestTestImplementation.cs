using Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.CallHandlers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.HttpRequests.TestRequestImplementations;

public class HttpPostRequestTestImplementation : HttpPostRequest<ITestHttpClientCallHandler, object, object>
{
    public HttpPostRequestTestImplementation(object value, ITestHttpClientCallHandler callHandler) : base(value, callHandler) { }

    protected override string? Url => "/test/test";

    protected override Dictionary<string, string> QueryStringParameters => new() { { "test", "parameter" } };
}
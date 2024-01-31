using Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.CallHandlers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.HttpRequests;

public abstract class HttpRequestTestBase : HttpClientExtensionsBase
{
    protected readonly Mock<ITestHttpClientCallHandler> _httpCallHandler = new();
}
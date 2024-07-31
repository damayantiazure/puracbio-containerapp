using Microsoft.AspNetCore.Http;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests.Helpers;
public static class HttpRequestHelpers
{
    public static HttpRequest CreateHttpRequestMock(string Token)
    {
        var httpRequest = new Mock<HttpRequest>();

        IHeaderDictionary headers = new HeaderDictionary { { "Authorization", Token } };
        httpRequest.SetupGet(x => x.Headers)
            .Returns(headers);

        return httpRequest.Object;
    }
}
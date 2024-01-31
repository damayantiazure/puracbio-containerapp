using Microsoft.AspNetCore.Http;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class FunctionTestBase
{
    protected readonly IFixture _fixture = new Fixture();
    private static string _token;

    protected FunctionTestBase()
    {
        _token = _fixture.Create<string>();
    }

    protected static HttpRequest CreateHttpRequestMock()
    {
        var httpRequest = new Mock<HttpRequest>();

        IHeaderDictionary headers = new HeaderDictionary { { "Authorization", _token } };
        httpRequest.SetupGet(x => x.Headers)
            .Returns(headers);

        return httpRequest.Object;
    }
}
using System.Net.Http;

namespace Rabobank.Compliancy.Functions.Shared.Tests;

public class FunctionRequestTests
{
    private const string TESTURL = "https://test.test.test/test/test.xml";
    private HttpRequestMessage _testRequest = null;
    public HttpRequestMessage TestRequest => _testRequest ??= new HttpRequestMessage(HttpMethod.Post, TESTURL);
}
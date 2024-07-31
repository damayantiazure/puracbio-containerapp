using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Flurl.Http;
using RichardSzalay.MockHttp;
using Shouldly;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests;

public class RetryPolicyTests
{
    [Theory]
    [InlineData(HttpStatusCode.RequestTimeout)] // 408
    [InlineData(HttpStatusCode.BadGateway)] // 502
    [InlineData(HttpStatusCode.ServiceUnavailable)] // 503
    [InlineData(HttpStatusCode.GatewayTimeout)] // 504
    public async Task ShouldRetryOnRetryableStatusCodes(HttpStatusCode statusCode)
    {
        var call = new HttpCall
        {
            Response = new HttpResponseMessage
            {
                StatusCode = statusCode
            }
        };
        var ex = new FlurlHttpException(call, "", new Exception());

        var url = "http://www.bla.com";
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Get, url)
            .Throw(ex);
        mockHttp.Expect(HttpMethod.Get, url)
            .Respond(HttpStatusCode.OK);

        var retryPolicy = AzdoHttpPolicies.GetRetryPolicyAsync();
        using (var client = new HttpClient(mockHttp))
        {
            await retryPolicy.ExecuteAsync(() =>
            {
                return client.GetAsync(url);
            });
        }
        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ShouldNotRetryOnSuccess()
    {
        var url = "http://www.bla.com";
        var mockHttp = new MockHttpMessageHandler();
        var request = mockHttp.When(HttpMethod.Get, url)
            .Respond(HttpStatusCode.OK);

        using (var client = new HttpClient(mockHttp))
        {
            (await client.GetAsync(url)).Dispose();
        }
        mockHttp.GetMatchCount(request).ShouldBe(1);
    }

    [Fact]
    public async Task ShouldRetryOnRefusedConnection()
    {
        var ex = new SocketException((int)SocketError.ConnectionRefused);

        var url = "http://www.bla.com";
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Get, url)
            .Throw(ex);
        mockHttp.Expect(HttpMethod.Get, url)
            .Respond(HttpStatusCode.OK);

        var retryPolicy = AzdoHttpPolicies.GetRetryPolicyAsync();
        using (var client = new HttpClient(mockHttp))
        {
            await retryPolicy.ExecuteAsync(() =>
            {
                return client.GetAsync(url);
            });
        }
        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ShouldRetryOnWrappedSockedException()
    {
        var ex = new Exception("outer", new Exception("more inner", new SocketException((int)SocketError.ConnectionRefused)));

        var url = "http://www.bla.com";
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Get, url)
            .Throw(ex);
        mockHttp.Expect(HttpMethod.Get, url)
            .Respond(HttpStatusCode.OK);

        HttpResponseMessage response = null;
        var retryPolicy = AzdoHttpPolicies.GetRetryPolicyAsync();
        using (var client = new HttpClient(mockHttp))
        {
            response = await retryPolicy.ExecuteAsync(() =>
            {
                return client.GetAsync(url);
            });
        }
        mockHttp.VerifyNoOutstandingExpectation();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
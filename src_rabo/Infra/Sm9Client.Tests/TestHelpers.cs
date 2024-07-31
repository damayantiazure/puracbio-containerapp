using Moq.Protected;
using Newtonsoft.Json;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;
using System.Net;

namespace Rabobank.Compliancy.Infra.Sm9Client.Tests;

internal static class TestHelpers
{
    private const string SendAsync = nameof(SendAsync);
    private static readonly Fixture Fixture = new();

    internal static Mock<HttpMessageHandler> SetupSuccessHttpMessageHandlerMock<TRequestContent>(
        string path, HttpMethod httpMethod, Func<TRequestContent, bool> matchRequestContent, object responseObj)
    {
        var messageHandlerMock = new Mock<HttpMessageHandler>();
        messageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(SendAsync,
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.IsMatch(path, httpMethod, matchRequestContent)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateHttpResponseMessage(HttpStatusCode.OK, responseObj))
            .Verifiable();

        return messageHandlerMock;
    }

    internal static Mock<HttpMessageHandler> SetupSuccessHttpMessageHandlerMock(
        string path, HttpMethod httpMethod, object responseObj)
    {
        var messageHandlerMock = new Mock<HttpMessageHandler>();
        messageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(SendAsync,
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.IsMatch(path, httpMethod)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateHttpResponseMessage(HttpStatusCode.OK, responseObj))
            .Verifiable();

        return messageHandlerMock;
    }

    internal static Mock<HttpMessageHandler> SetupSequenceSuccessHttpMessageHandlerMock(
        string path, HttpMethod httpMethod, object? responseObj, Mock<HttpMessageHandler>? messageHandlerMock = null)
    {
        messageHandlerMock ??= new Mock<HttpMessageHandler>();

        messageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(SendAsync,
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.IsMatch(path, httpMethod)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateHttpResponseMessage(HttpStatusCode.OK, responseObj));

        return messageHandlerMock;
    }

    internal static Mock<HttpMessageHandler> SetupSequenceSuccessHttpMessageHandlerMock<TRequestContent>(
        string path, HttpMethod httpMethod, Func<TRequestContent, bool> matchRequestContent, object? responseObj, Mock<HttpMessageHandler>? messageHandlerMock = null)
    {
        messageHandlerMock ??= new Mock<HttpMessageHandler>();

        messageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(SendAsync,
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.IsMatch(path, httpMethod, matchRequestContent)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateHttpResponseMessage(HttpStatusCode.OK, responseObj));

        return messageHandlerMock;
    }

    internal static Mock<HttpMessageHandler> SetupFailHttpMessageHandlerMock(string path, HttpMethod httpMethod)
    {
        var messageHandlerMock = new Mock<HttpMessageHandler>();
        messageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(SendAsync,
                ItExpr.Is<HttpRequestMessage>(requestMessage =>
                    requestMessage.IsMatch(path, httpMethod)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateHttpResponseMessage(HttpStatusCode.NotFound))
            .Verifiable();

        return messageHandlerMock;
    }

    internal static Mock<IHttpClientFactory> SetupHttpClientFactoryMock(
        string httpClientName, HttpMessageHandler httpMessageHandler)
    {
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(httpClientFactory => httpClientFactory.CreateClient(httpClientName))
            .Returns(new HttpClient(httpMessageHandler)
            {
                BaseAddress = Fixture.Create<Uri>()
            });

        return httpClientFactoryMock;
    }

    private static HttpResponseMessage
        CreateHttpResponseMessage(HttpStatusCode statusCode, object? responseObj = null) =>
        responseObj == null
            ? new HttpResponseMessage { StatusCode = statusCode }
            : new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(responseObj))
            };

    internal static async Task<T?> ToObjectAsync<T>(this HttpContent httpContent) =>
        JsonConvert.DeserializeObject<T>(await httpContent.ReadAsStringAsync());

    internal static GetCiResponse CreateCiContentItemResponse(int count)
    {
        var contentItems = new List<CiContentItem>();

        for (var i = 1; i <= count; i++)
        {
            contentItems.Add(new CiContentItem
            {
                Device = new ConfigurationItemModel
                {
                    CiIdentifier = $"CiIdentifier{i}",
                    AssignmentGroup = $"SystemOwner{i}",
                    Status = "In Use - Production",
                    Environment = new[] { "Production" }
                }
            });
        }

        return new GetCiResponse
        {
            Content = contentItems
        };
    }

    private static bool IsMatch(this HttpRequestMessage request, string path, HttpMethod httpMethod) =>
        request.RequestUri!.ToString().Contains(path) &&
        request.Method == httpMethod;

    private static bool IsMatch<TRequestContent>(this HttpRequestMessage request, string path,
        HttpMethod httpMethod, Func<TRequestContent, bool> matchRequestContent) =>
        request.RequestUri!.ToString().Contains(path) &&
        request.Method == httpMethod &&
        matchRequestContent(request.Content!.ToObjectAsync<TRequestContent>().GetAwaiter().GetResult()!);
}
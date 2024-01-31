using FluentAssertions;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.Tests;

public class HttpClientCallHandlerTests : HttpClientExtensionsBase
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();
    private readonly MockHttpMessageHandler _messageHandler = new();
    private HttpClient? _httpClient;

    [Fact]
    public async Task HandleGetCall_ReturnsExpectedResult()
    {
        // Arrange
        var expectedResult = new HttpJsonSerializationTest { Id = 321, Name = InvariantUnitTestValue };
        _messageHandler.When(HttpMethod.Get, InvariantUnitTestBaseUrl)
            .Respond(HttpStatusCode.OK, JsonContent.Create(expectedResult));
        var sut = CreateSut();

        // Act
        var actual =
            await sut.HandleGetCallAsync<HttpJsonSerializationTest>(new Uri(InvariantUnitTestBaseUrl));

        // Assert        
        actual.Should().NotBeNull();
        actual!.Id.Should().Be(expectedResult.Id);
        actual.Name.Should().Be(expectedResult.Name);
    }

    [Fact]
    public async Task HandlePostCall_ReturnsExpectedResult()
    {
        // Arrange
        var expectedResult = new HttpJsonSerializationTest { Id = 321, Name = InvariantUnitTestValue };
        _messageHandler.When(HttpMethod.Post, InvariantUnitTestBaseUrl + InvariantUnitTestValue)
            .Respond(HttpStatusCode.OK, JsonContent.Create(expectedResult));
        var sut = CreateSut();

        // Act
        var actual =
            await sut.HandlePostCallAsync<HttpJsonSerializationTest, string>(
                new Uri(InvariantUnitTestValue, UriKind.Relative), InvariantUnitTestValue);

        // Assert
        actual.Should().NotBeNull();
        actual!.Id.Should().Be(expectedResult.Id);
        actual.Name.Should().Be(expectedResult.Name);
    }

    [Fact]
    public async Task HandlePutCall_ReturnsExpectedResult()
    {
        // Arrange
        var expectedResult = new HttpJsonSerializationTest { Id = 321, Name = InvariantUnitTestValue };
        _messageHandler.When(HttpMethod.Put, InvariantUnitTestBaseUrl + InvariantUnitTestValue)
            .Respond(HttpStatusCode.OK, JsonContent.Create(expectedResult));
        var sut = CreateSut();

        // Act
        var actual =
            await sut.HandlePutCallAsync<HttpJsonSerializationTest, string>(
                new Uri(InvariantUnitTestValue, UriKind.Relative), InvariantUnitTestValue);

        // Assert
        actual.Should().NotBeNull();
        actual!.Id.Should().Be(expectedResult.Id);
        actual.Name.Should().Be(expectedResult.Name);
    }

    [Fact]
    public async Task HandleDeleteCall_ReturnsExpectedResult()
    {
        // Arrange
        var expectedResult = new HttpJsonSerializationTest { Id = 321, Name = InvariantUnitTestValue };
        _messageHandler.When(HttpMethod.Delete, InvariantUnitTestBaseUrl + InvariantUnitTestValue)
            .Respond(HttpStatusCode.OK, JsonContent.Create(expectedResult));
        var sut = CreateSut();

        // Act
        var actual =
            await sut.HandleDeleteCallAsync<HttpJsonSerializationTest>(
                new Uri(InvariantUnitTestValue, UriKind.Relative));

        // Assert
        actual.Should().NotBeNull();
        actual!.Id.Should().Be(expectedResult.Id);
        actual.Name.Should().Be(expectedResult.Name);
    }

    [Fact]
    public async Task HandlePatchCall_ReturnsExpectedResult()
    {
        // Arrange
        var expectedResult = new HttpJsonSerializationTest { Id = 321, Name = InvariantUnitTestValue };
        _messageHandler.When(HttpMethod.Patch, InvariantUnitTestBaseUrl + InvariantUnitTestValue)
            .Respond(HttpStatusCode.OK, JsonContent.Create(expectedResult));
        var sut = CreateSut();

        // Act
        var actual =
            await sut.HandlePatchCallAsync<HttpJsonSerializationTest, string>(
                new Uri(InvariantUnitTestValue, UriKind.Relative), InvariantUnitTestValue);

        // Assert
        actual.Should().NotBeNull();
        actual!.Id.Should().Be(expectedResult.Id);
        actual.Name.Should().Be(expectedResult.Name);
    }

    [Fact]
    public async Task HandleDeleteCall_UsesCustomToken()
    {
        // Arrange
        var expectedResult = new HttpJsonSerializationTest { Id = 321, Name = InvariantUnitTestValue };
        _messageHandler.When(HttpMethod.Delete, InvariantUnitTestBaseUrl + InvariantUnitTestValue)
            .Respond(HttpStatusCode.OK, JsonContent.Create(expectedResult));
        var sut = CreateSut();

        // Act
        await sut.HandleDeleteCallAsync<HttpJsonSerializationTest>(new Uri(InvariantUnitTestValue, UriKind.Relative),
            new AuthenticationHeaderValue("Bearer", InvariantUnitTestValue));

        // Assert
        _httpClient?.DefaultRequestHeaders.Authorization?.ToString().Should().BeEquivalentTo($"Bearer {InvariantUnitTestValue}");
    }

    /// <summary>
    ///     Instantiates a HttpClientCallHandler with a httpClientFactory which will create out mocked message handler.
    ///     Uses the InvariantUnitTestValue as our token (which is also the name of the client).
    /// </summary>
    /// <returns></returns>
    private HttpClientCallHandler CreateSut()
    {
        _httpClient = new HttpClient(_messageHandler) { BaseAddress = new Uri(InvariantUnitTestBaseUrl) };
        _httpClientFactory.Setup(factory => factory.CreateClient(InvariantUnitTestValue)).Returns(_httpClient);
        return new HttpClientCallHandler(_httpClientFactory.Object, InvariantUnitTestValue);
    }

    private class HttpJsonSerializationTest
    {
        public int Id { get; init; }
        public string? Name { get; init; }
    }
}
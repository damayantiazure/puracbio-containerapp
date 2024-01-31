using Rabobank.Compliancy.Infra.Sm9Client.Change;
using Rabobank.Compliancy.Infra.Sm9Client.Change.Model;

namespace Rabobank.Compliancy.Infra.Sm9Client.Tests.Change;

public class ChangeClientTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task CreateChangeAsync_CallToHttpClient_ShouldBeSuccessful()
    {
        // Arrange
        var responseObj = _fixture.Create<CreateChangeResponse>();
        const string path = "/createChange";
        var httpMethod = HttpMethod.Post;

        var messageHandlerMock = TestHelpers.SetupSuccessHttpMessageHandlerMock(path, httpMethod, responseObj);
        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(ChangeClient), messageHandlerMock.Object);

        var sut = new ChangeClient(httpClientFactoryMock.Object);

        var requestBody = _fixture.Create<CreateChangeRequestBody>();

        // Act
        var actual = await sut.CreateChangeAsync(requestBody);

        // Assert
        messageHandlerMock.Verify();
        actual.Should().BeEquivalentTo(responseObj);
    }

    [Fact]
    public async Task CreateChangeAsync_CallToHttpClient_ShouldFail()
    {
        // Arrange
        const string path = "/createChange";
        var httpMethod = HttpMethod.Post;

        var messageHandlerMock = TestHelpers.SetupFailHttpMessageHandlerMock(path, httpMethod);
        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(ChangeClient), messageHandlerMock.Object);

        var sut = new ChangeClient(httpClientFactoryMock.Object);

        var requestBody = _fixture.Create<CreateChangeRequestBody>();

        // Act
        var actual = () => sut.CreateChangeAsync(requestBody);

        // Assert
        await actual.Should().ThrowAsync<ChangeClientException>();
        messageHandlerMock.Verify();
    }

    [Fact]
    public async Task CloseChangeAsync_CallToHttpClient_ShouldBeSuccessful()
    {
        // Arrange
        var responseObj = _fixture.Create<CloseChangeResponse>();
        const string path = "/closeChange";
        var httpMethod = HttpMethod.Post;

        var messageHandlerMock = TestHelpers.SetupSuccessHttpMessageHandlerMock(path, httpMethod, responseObj);
        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(ChangeClient), messageHandlerMock.Object);

        var sut = new ChangeClient(httpClientFactoryMock.Object);

        var requestBody = _fixture.Create<CloseChangeRequestBody>();

        // Act
        var actual = await sut.CloseChangeAsync(requestBody);

        // Assert
        messageHandlerMock.Verify();
        actual.Should().BeEquivalentTo(responseObj);
    }

    [Fact]
    public async Task CloseChangeAsync_CallToHttpClient_ShouldFail()
    {
        // Arrange
        const string path = "/closeChange";
        var httpMethod = HttpMethod.Post;

        var messageHandlerMock = TestHelpers.SetupFailHttpMessageHandlerMock(path, httpMethod);
        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(ChangeClient), messageHandlerMock.Object);

        var sut = new ChangeClient(httpClientFactoryMock.Object);

        var requestBody = _fixture.Create<CloseChangeRequestBody>();

        // Act
        var actual = () => sut.CloseChangeAsync(requestBody);

        // Assert
        await actual.Should().ThrowAsync<ChangeClientException>();
        messageHandlerMock.Verify();
    }

    [Fact]
    public async Task UpdateChangeAsync_CallToHttpClient_ShouldBeSuccessful()
    {
        // Arrange
        var responseObj = _fixture.Create<UpdateChangeResponse>();
        const string path = "/updateChange";
        var httpMethod = HttpMethod.Post;

        var messageHandlerMock = TestHelpers.SetupSuccessHttpMessageHandlerMock(path, httpMethod, responseObj);
        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(ChangeClient), messageHandlerMock.Object);

        var sut = new ChangeClient(httpClientFactoryMock.Object);

        var requestBody = _fixture.Create<UpdateChangeRequestBody>();

        // Act
        var actual = await sut.UpdateChangeAsync(requestBody);

        // Assert
        messageHandlerMock.Verify();
        actual.Should().BeEquivalentTo(responseObj);
    }

    [Fact]
    public async Task UpdateChangeAsync_CallToHttpClient_ShouldFail()
    {
        // Arrange
        const string path = "/updateChange";
        var httpMethod = HttpMethod.Post;

        var messageHandlerMock = TestHelpers.SetupFailHttpMessageHandlerMock(path, httpMethod);
        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(ChangeClient), messageHandlerMock.Object);

        var sut = new ChangeClient(httpClientFactoryMock.Object);

        var requestBody = _fixture.Create<UpdateChangeRequestBody>();

        // Act
        var actual = () => sut.UpdateChangeAsync(requestBody);

        // Assert
        await actual.Should().ThrowAsync<ChangeClientException>();
        messageHandlerMock.Verify();
    }

    [Fact]
    public async Task GetChangeByKeyAsync_CallToHttpClient_ShouldBeSuccessful()
    {
        // Arrange
        var responseObj = _fixture.Create<GetChangeByKeyResponse>();
        const string path = "/retrieveChangeInfoByKey";
        var httpMethod = HttpMethod.Post;

        var messageHandlerMock = TestHelpers.SetupSuccessHttpMessageHandlerMock(path, httpMethod, responseObj);
        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(ChangeClient), messageHandlerMock.Object);

        var sut = new ChangeClient(httpClientFactoryMock.Object);

        var requestBody = _fixture.Create<GetChangeByKeyRequestBody>();

        // Act
        var actual = await sut.GetChangeByKeyAsync(requestBody);

        // Assert
        messageHandlerMock.Verify();
        actual.Should().BeEquivalentTo(responseObj);
    }

    [Fact]
    public async Task GetChangeByKeyAsync_CallToHttpClient_ShouldFail()
    {
        // Arrange
        const string path = "/retrieveChangeInfoByKey";
        var httpMethod = HttpMethod.Post;

        var messageHandlerMock = TestHelpers.SetupFailHttpMessageHandlerMock(path, httpMethod);
        var httpClientFactoryMock =
            TestHelpers.SetupHttpClientFactoryMock(nameof(ChangeClient), messageHandlerMock.Object);

        var sut = new ChangeClient(httpClientFactoryMock.Object);

        var requestBody = _fixture.Create<GetChangeByKeyRequestBody>();

        // Act
        var actual = () => sut.GetChangeByKeyAsync(requestBody);

        // Assert
        await actual.Should().ThrowAsync<ChangeClientException>();
        messageHandlerMock.Verify();
    }
}
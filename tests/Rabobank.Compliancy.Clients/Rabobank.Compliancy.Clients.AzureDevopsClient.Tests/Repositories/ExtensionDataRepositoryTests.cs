using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.TestImplementations;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class ExtensionDataRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IExtmgmtHttpClientCallHandler> _httpClientCallHandlerMock
        =
        new();

    private readonly ExtensionDataRepository _sut;

    public ExtensionDataRepositoryTests() =>
        _sut = new ExtensionDataRepository(_httpClientCallHandlerMock.Object);

    [Fact]
    public async Task UploadAsync_ShouldCallHttpClientCallDistributor()
    {
        // Arrange
        var publisher = _fixture.Create<string>();
        var collection = _fixture.Create<string>();
        var extensionName = _fixture.Create<string>();
        var organization = _fixture.Create<string>();
        var extensionData = _fixture.Create<ExtensionDataTestImplementation>();
        var expectedUrlPath =
            $"{organization}/_apis/ExtensionManagement/InstalledExtensions/{publisher}/{extensionName}" +
            $"/Data/Scopes/Default/Current/Collections/{collection}/Documents?api-version=6.1-preview";

        _httpClientCallHandlerMock.Setup(
                m => m.HandlePutCallAsync<ExtensionDataTestImplementation, ExtensionDataTestImplementation>(
                    It.Is<Uri>(p => p.ToString().EndsWith(expectedUrlPath)), extensionData,
                    It.IsAny<AuthenticationHeaderValue>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(extensionData);

        // Act
        var actual = await _sut.UploadAsync(
            publisher, collection, extensionName, organization, extensionData);

        // Assert
        _httpClientCallHandlerMock.Verify(
            m => m.HandlePutCallAsync<ExtensionDataTestImplementation, ExtensionDataTestImplementation>(
                It.Is<Uri>(p => p.ToString().EndsWith(expectedUrlPath)), extensionData,
                It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()), Times.Once);

        actual.Should().BeEquivalentTo(extensionData);
    }

    [Fact]
    public async Task DownloadAsync_ShouldCallHttpClientCallDistributor()
    {
        // Arrange
        var publisher = _fixture.Create<string>();
        var collection = _fixture.Create<string>();
        var extensionName = _fixture.Create<string>();
        var organization = _fixture.Create<string>();
        var id = _fixture.Create<string>();
        var extensionData = _fixture.Create<ExtensionDataTestImplementation>();
        var expectedUrlPath =
            $"{organization}/_apis/ExtensionManagement/InstalledExtensions/{publisher}/{extensionName}" +
            $"/Data/Scopes/Default/Current/Collections/{collection}/Documents/{id}?api-version=6.1-preview";

        _httpClientCallHandlerMock.Setup(
                m => m.HandleGetCallAsync<ExtensionDataTestImplementation>(
                    It.Is<Uri>(p => p.ToString().EndsWith(expectedUrlPath)),
                    It.IsAny<AuthenticationHeaderValue>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(extensionData);

        // Act
        var actual = await _sut.DownloadAsync<ExtensionDataTestImplementation>(
            publisher, collection, extensionName, organization, id);

        // Assert
        _httpClientCallHandlerMock.Verify(
            m => m.HandleGetCallAsync<ExtensionDataTestImplementation>(
                It.Is<Uri>(p => p.ToString().EndsWith(expectedUrlPath)),
                It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()), Times.Once);

        actual.Should().BeEquivalentTo(extensionData);
    }
}
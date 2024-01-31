using Microsoft.VisualStudio.Services.Operations;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class OperationsRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IDevHttpClientCallHandler> _httpClientCallHandlerMock = new();

    private readonly OperationsRepository _sut;

    public OperationsRepositoryTests()
    {
        _sut = new OperationsRepository(_httpClientCallHandlerMock.Object);
    }

    [Fact]
    public async Task GetOperationReferenceByIdAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var operation = _fixture.Create<Operation?>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<Operation?>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operation);

        // Act
        var actual = await _sut.GetOperationReferenceByIdAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(operation);
    }

    [Theory]
    [InlineData(OperationStatus.NotSet)]
    [InlineData(OperationStatus.Queued)]
    [InlineData(OperationStatus.InProgress)]
    public async Task OperationIsInProgressAsync_WithoutFinalStatus_ShouldReturnInProgress(OperationStatus operationStatus)
    {
        // Arrange
        var operation = _fixture.Create<Operation?>();
        operation!.Status = operationStatus;

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<Operation?>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operation);

        // Act
        var actual = await _sut.OperationIsInProgressAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeTrue();
    }

    [Theory]
    [InlineData(OperationStatus.Cancelled)]
    [InlineData(OperationStatus.Succeeded)]
    [InlineData(OperationStatus.Failed)]
    public async Task OperationIsInProgressAsync_WithFinalStatus_ShouldReturnNotInProgress(OperationStatus operationStatus)
    {
        // Arrange
        var operation = _fixture.Create<Operation?>();
        operation!.Status = operationStatus;

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<Operation?>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operation);

        // Act
        var actual = await _sut.OperationIsInProgressAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeFalse();
    }


    [Fact]
    public async Task OperationIsInProgressAsync_WhenOperationNotFound_ShouldReturnFalse()
    {
        // Nothing to arrange, the mock should return null

        // Act
        var actual = await _sut.OperationIsInProgressAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeFalse();
    }
}
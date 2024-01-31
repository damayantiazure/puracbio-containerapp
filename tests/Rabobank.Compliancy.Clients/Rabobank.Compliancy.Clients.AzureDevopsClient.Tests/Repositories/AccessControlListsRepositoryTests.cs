using Microsoft.VisualStudio.Services.Security;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class AccessControlListsRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IDevHttpClientCallHandler> _httpClientCallHandlerMock = new();

    private readonly AccessControlListsRepository _sut;

    public AccessControlListsRepositoryTests()
    {
        _sut = new AccessControlListsRepository(_httpClientCallHandlerMock.Object);
    }

    [Fact]
    public async Task GetAccessControlListsForProjectAndSecurityNamespaceAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var accessControlList = _fixture.Create<AccessControlList>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<AccessControlList>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseCollection<AccessControlList> { Count = 1, Value = new[] { accessControlList } }).Verifiable();

        // Act
        var actual = await _sut.GetAccessControlListsForProjectAndSecurityNamespaceAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeNull();
        actual!.First().Should().BeEquivalentTo(accessControlList);
        _httpClientCallHandlerMock.Verify();
    }
}
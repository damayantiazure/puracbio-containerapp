using Microsoft.VisualStudio.Services.Identity;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class IdentityRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IVsspsHttpClientCallHandler> _httpClientCallHandlerMock = new();

    private readonly IdentityRepository _sut;

    public IdentityRepositoryTests()
    {
        _sut = new IdentityRepository(_httpClientCallHandlerMock.Object);
    }

    [Fact]
    public async Task GetIdentitiesForIdentityDescriptorsAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var identity = new Identity { Descriptor = _fixture.Create<IdentityDescriptor>() };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<Identity>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseCollection<Identity> { Count = 1, Value = new[] { identity } }).Verifiable();

        // Act
        var actual = await _sut.GetIdentitiesForIdentityDescriptorsAsync(_fixture.Create<string>(), _fixture.CreateMany<IdentityDescriptor>(), QueryMembership.Expanded, It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeNull();
        actual!.First().Should().BeEquivalentTo(identity);
        _httpClientCallHandlerMock.Verify();
    }
}
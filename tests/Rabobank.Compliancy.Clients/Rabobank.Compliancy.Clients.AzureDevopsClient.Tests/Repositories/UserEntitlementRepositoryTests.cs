using Microsoft.VisualStudio.Services.Graph.Client;
using Microsoft.VisualStudio.Services.MemberEntitlementManagement.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.FixtureCustomizations;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class UserEntitlementRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IVsaexHttpClientCallHandler> _httpClientCallHandlerMock = new();

    private readonly UserEntitlementRepository _sut;

    public UserEntitlementRepositoryTests()
    {
        _fixture.Customizations.Add(new NonPublicConstructorBuilder<GraphUser>());
        _sut = new UserEntitlementRepository(_httpClientCallHandlerMock.Object);
    }

    [Fact]
    public async Task GetUserEntitlementByIdAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedUserEntitlementInstance()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var userId = _fixture.Create<Guid>();
        var user = _fixture.Create<GraphUser>();

        var userEntitlement = _fixture.Build<UserEntitlement>().With(x => x.User, user).Without(x => x.GraphMember).Without(x => x.GroupAssignments).Without(x => x.ProjectEntitlements).Create();
        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<UserEntitlement?>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userEntitlement);

        // Act
        var actual = await _sut.GetUserEntitlementByIdAsync(organization, userId);

        // Assert
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(userEntitlement);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetUserEntitlementByIdAsync_WithNoRecordFound_ShouldReturnNull()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var userId = _fixture.Create<Guid>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<UserEntitlement?>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserEntitlement?)null);

        // Act
        var actual = await _sut.GetUserEntitlementByIdAsync(organization, userId);

        // Assert
        actual.Should().BeNull();
        _httpClientCallHandlerMock.Verify();
    }
}
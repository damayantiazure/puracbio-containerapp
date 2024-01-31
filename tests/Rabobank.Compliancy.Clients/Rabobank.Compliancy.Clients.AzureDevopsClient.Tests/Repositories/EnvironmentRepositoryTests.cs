using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.Gallery.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class EnvironmentRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IDevHttpClientCallHandler> _httpClientCallHandlerMock = new();

    private readonly EnvironmentRepository _sut;

    public EnvironmentRepositoryTests() =>
        _sut = new EnvironmentRepository(_httpClientCallHandlerMock.Object);

    [Fact]
    public async Task GetEnvironmentsAsync_WithCorrectParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();

        var releaseDefinitions = new ResponseCollection<EnvironmentInstance>
        {
            Count = 2,
            Value = new[]
            {
                new EnvironmentInstance { Id = _fixture.Create<int>() },
                new EnvironmentInstance { Id = _fixture.Create<int>() }
            }
        };

        _httpClientCallHandlerMock.Setup(callDistributor =>
                callDistributor.HandleGetCallAsync<ResponseCollection<EnvironmentInstance>>(It.IsAny<Uri>(),
                    It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(releaseDefinitions).Verifiable();

        // Act
        var actual = await _sut.GetEnvironmentsAsync(organization, projectId,
            It.IsAny<CancellationToken>());

        // Assert
        actual!.Select(a => a.Id).Should().BeEquivalentTo(releaseDefinitions.Value.Select(r => r.Id));
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task SetSecurityGroupsAsync_WithCorrectParameters_ShouldReturnExpectedResponse()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var scopeId = _fixture.Create<string>();
        var resourceId = _fixture.Create<string>();
        var identityId = _fixture.Create<string>();
        var content = _fixture.Create<RoleAssignmentBodyContent>();
        var roleAssignment = new PublisherRoleAssignment { Role = _fixture.Create<PublisherSecurityRole>() };

        _httpClientCallHandlerMock.Setup(callDistributor =>
                callDistributor.HandlePutCallAsync<PublisherRoleAssignment, RoleAssignmentBodyContent>(
                    It.IsAny<Uri>(),
                    content, It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roleAssignment).Verifiable();

        // Act
        var actual = await _sut.SetSecurityGroupsAsync(organization, scopeId, resourceId, identityId, content,
            It.IsAny<CancellationToken>());

        // Assert
        actual!.Role.Should().BeEquivalentTo(roleAssignment.Role);
        _httpClientCallHandlerMock.Verify();
    }
}
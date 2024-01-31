using System;
using System.Net.Http;
using System.Net.Http.Headers;
using AutoFixture;
using FluentAssertions;
using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using Xunit;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Bits = Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Tests.Services;

public class AuthorizationServiceTests
{
    private readonly IMemoryCache _cache = Create.MockedMemoryCache();
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetInteractiveUserAsync_WithValidArgs_ShouldReturnUser()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var userMailAddress = _fixture.Create<string>();
        var userId = Guid.NewGuid().ToString();

        var azdoClient = new Mock<IAzdoRestClient>();
        azdoClient.Setup(azdoRestClient => azdoRestClient.GetWithTokenAsync(
                It.IsAny<IAzdoRequest<ConnectionData>>(), It.IsAny<string>(), organization))
            .ReturnsAsync(new ConnectionData
            {
                AuthorizedUser = new Identity
                {
                    Id = userId,
                    Properties = new IdentityProperties
                    {
                        Account = new IdentityProperty
                        {
                            Value = userMailAddress
                        }
                    }
                }
            });

        var request = new HttpRequestMessage();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "TestToken");
        var authorizationService = new AuthorizationService(azdoClient.Object, _cache);

        // Act
        var actual = await authorizationService.GetInteractiveUserAsync(request, organization);

        // Assert
        actual.MailAddress.Should().Be(userMailAddress);
        actual.UniqueId.Should().Be(userId);
    }

    [Fact]
    public async Task HasEditPermissionsAsync_ThrowsArgumentException_WithoutUser()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();

        var azdoClient = new Mock<IAzdoRestClient>();

        var request = new HttpRequestMessage();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "TestToken");
        var authorizationService = new AuthorizationService(azdoClient.Object, _cache);

        // Act
        var actual = () => authorizationService.HasEditPermissionsAsync(
            request, organization, projectId, pipelineId, ItemTypes.ClassicReleasePipeline);

        // Assert
        await actual.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HasEditPermissionsAsync_PipelineNotFound_ThrowsItemNotFoundException()
    {
        //Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();

        var azdoClient = new Mock<IAzdoRestClient>();
        azdoClient.Setup(a =>
                a.GetWithTokenAsync(It.IsAny<IAzdoRequest<ConnectionData>>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<ConnectionData>());

        var request = new HttpRequestMessage();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "TestToken");
        var authorizationService = new AuthorizationService(azdoClient.Object, _cache);

        //Act
        var actual = () => authorizationService.HasEditPermissionsAsync(
            request, organization, projectId, pipelineId, ItemTypes.ClassicReleasePipeline);

        // Assert
        await actual.Should().ThrowAsync<ItemNotFoundException>();
    }

    [Theory]
    [InlineData(PermissionLevelId.Allow, ItemTypes.YamlReleasePipeline, true)]
    [InlineData(PermissionLevelId.AllowInherited, ItemTypes.YamlReleasePipeline, true)]
    [InlineData(PermissionLevelId.Deny, ItemTypes.YamlReleasePipeline, false)]
    [InlineData(PermissionLevelId.NotSet, ItemTypes.YamlReleasePipeline, false)]
    [InlineData(PermissionLevelId.Allow, ItemTypes.ClassicReleasePipeline, true)]
    [InlineData(PermissionLevelId.AllowInherited, ItemTypes.ClassicReleasePipeline, true)]
    [InlineData(PermissionLevelId.Deny, ItemTypes.ClassicReleasePipeline, false)]
    [InlineData(PermissionLevelId.NotSet, ItemTypes.ClassicReleasePipeline, false)]
    public async Task HasEditPermissionsAsync_WithEditPermission_ReturnsTrue(
        int permissionBit, string pipelineType, bool hasPermission)
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();

        var azdoClient = new Mock<IAzdoRestClient>();

        azdoClient.Setup(azdoRestClient =>
                azdoRestClient.GetWithTokenAsync(It.IsAny<IAzdoRequest<ConnectionData>>(), It.IsAny<string>(),
                    It.IsAny<string>()))
            .ReturnsAsync(_fixture.Build<ConnectionData>()
                .With(f => f.AuthorizedUser, _fixture.Build<Identity>()
                    .With(f => f.Id, _fixture.Create<Guid>().ToString)
                    .Create())
                .Create());

        azdoClient.Setup(azdoRestClient =>
                azdoRestClient.GetAsync(It.IsAny<IAzdoRequest<PermissionsProjectId>>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<PermissionsProjectId>());

        azdoClient
            .Setup(azdoRestClient =>
                azdoRestClient.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<BuildDefinition>());

        azdoClient
            .Setup(azdoRestClient =>
                azdoRestClient.GetAsync(It.IsAny<IAzdoRequest<ReleaseDefinition>>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<ReleaseDefinition>());

        azdoClient
            .Setup(azdoRestClient =>
                azdoRestClient.GetAsync(It.IsAny<IAzdoRequest<PermissionsSet>>(), It.IsAny<string>()))
            .ReturnsAsync(new PermissionsSet
            {
                Permissions = new[]
                {
                    new Permission
                    {
                        DisplayName = "Edit build pipeline",
                        PermissionBit = Bits.BuildDefinitionBits.EditBuildPipeline,
                        NamespaceId = SecurityNamespaceIds.Build,
                        PermissionId = permissionBit
                    },
                    new Permission
                    {
                        DisplayName = "Edit release pipeline",
                        PermissionBit = Bits.ReleaseDefinitionBits.EditReleasePipeline,
                        NamespaceId = SecurityNamespaceIds.Release,
                        PermissionId = permissionBit
                    }
                }
            });

        var request = new HttpRequestMessage();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "TestToken");

        // Act
        var authorizationService = new AuthorizationService(azdoClient.Object, _cache);
        var result =
            await authorizationService.HasEditPermissionsAsync(request, organization, projectId, pipelineId,
                pipelineType);

        // Assert
        result.ShouldBe(hasPermission);
    }

    [Fact]
    public async Task GetUserAsync_WithoutToken_ThrowsArgumentException()
    {
        ManageProjectPropertiesPermission(_fixture);
        var organization = _fixture.Create<string>();

        var azdoClient = new Mock<IAzdoRestClient>();
        azdoClient.Setup(a => a.GetWithTokenAsync(It.IsAny<IAzdoRequest<ConnectionData>>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ConnectionData { AuthorizedUser = new Identity { Id = Guid.NewGuid().ToString() } }).Verifiable();

        var request = new HttpRequestMessage();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "");

        var authorizationService = new AuthorizationService(azdoClient.Object, _cache);
        await Should.ThrowAsync<ArgumentException>(() => authorizationService.GetInteractiveUserAsync(request, organization));
    }

    private static void ManageProjectPropertiesPermission(IFixture fixture)
    {
        fixture.Customize<Permission>(ctx => ctx
            .With(x => x.DisplayName, "Manage project properties")
            .With(x => x.PermissionId, 3));
    }
}
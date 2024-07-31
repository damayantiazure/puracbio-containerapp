#nullable enable

using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Functions.Sm9Changes.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Linq;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Tests.Services;

public class AzdoReleaseApproverServiceTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void ReleaseWithArtifactFromDifferentProject_ShouldCallPRApproveService_WithCorrectProjectId()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var runId = _fixture.Create<string>();

        var release = _fixture.Create<Release>();
        release.Artifacts!.First().Type = "Build";
        release.Artifacts!.First().DefinitionReference.Version.Id = "BuildId";
        release.Artifacts!.First().DefinitionReference.Project.Id = "OtherProject";            

        var client = new Mock<IAzdoRestClient>();
        client
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Release>>(), organization))
            .ReturnsAsync(release);

        var classicReleaseApproverService = new Mock<IClassicReleaseApproverService>();
        var pullRequestApproverService = new Mock<IPullRequestApproverService>();

        // Act
        var service = new AzdoReleaseApproverService(client.Object, classicReleaseApproverService.Object, 
            pullRequestApproverService.Object);

        var function = service.GetApproversAsync(organization, projectId, runId);

        // Assert
        pullRequestApproverService
            .Verify(m => m.GetAllApproversAsync(
                    release.Artifacts!.First().DefinitionReference.Project.Id,
                    release.Artifacts!.First().DefinitionReference.Version.Id,
                    organization), 
                Times.Once);
    }

    [Fact]
    public void ReleaseWithoutBuildArtifacts_ShouldNotCallPRApproveService()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var runId = _fixture.Create<string>();

        var artifacts = _fixture.Build<ArtifactReference>()
            .CreateMany(0)
            .ToList();
        var release = _fixture.Build<Release>()
            .With(r => r.Artifacts, artifacts)
            .Create();

        var client = new Mock<IAzdoRestClient>();
        client
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Release>>(), organization))
            .ReturnsAsync(release);

        var classicReleaseApproverService = new Mock<IClassicReleaseApproverService>();
        var pullRequestApproverService = new Mock<IPullRequestApproverService>();

        // Act
        var service = new AzdoReleaseApproverService(client.Object, classicReleaseApproverService.Object,
            pullRequestApproverService.Object);

        var function = service.GetApproversAsync(organization, projectId, runId);

        // Assert
        pullRequestApproverService
            .Verify(m => m.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
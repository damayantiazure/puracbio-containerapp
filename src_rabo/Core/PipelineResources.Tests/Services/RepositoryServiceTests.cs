using Rabobank.Compliancy.Core.PipelineResources.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.PipelineResources.Tests.Services;

public class RepositoryServiceTests
{
    private const string Organization = "TestOrganization";
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public async Task ShouldReportCorrectRepoUrlForReposLinkedToReleasePipeline()
    {
        // Arrange
        var repository = _fixture.Create<Repository>();
        repository.Id = "Repo1";
        repository.Project.Id = "Project1";

        // Act
        var function = new RepositoryService(null);
        var result = await function.GetUrlAsync(Organization, It.IsAny<Project>(), repository);

        // Assert
        result.Should().Be(new Uri("https://dev.azure.com/TestOrganization/Project1/_git/Repo1"));
    }

    [Fact]
    public async Task ShouldReportCorrectRepoUrlForReposLinkedToBuildPipelines()
    {
        // Arrange
        var repository = _fixture.Build<Repository>()
            .Without(r => r.Project)
            .Create();
        repository.Id = "2";
        repository.Url = new Uri($"https://dev.azure.com/TestOrganization/ProjectB/_git/Repo2");

        var project = _fixture.Create<Project>();
        project.Id = "ProjectId";

        var client = new Mock<IAzdoRestClient>();
        client
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Project>>(), Organization))
            .ReturnsAsync(project);

        // Act
        var function = new RepositoryService(client.Object);
        var result = await function.GetUrlAsync(Organization, _fixture.Create<Project>(), repository);

        // Assert
        result.Should().Be(new Uri("https://dev.azure.com/TestOrganization/ProjectId/_git/2"));
    }

    [Fact]
    public async Task ShouldReportCorrectRepoUrlForRepositoryResources()
    {
        // Arrange
        var repository = _fixture.Build<Repository>()
            .With(r => r.Id, "RepoId")
            .With(r => r.Url, new Uri($"https://dev.azure.com/TestOrganization/ProjectId/_apis/git/repositories/RepoId"))
            .Create();
        repository.Project.Id = "ProjectId";

        var client = new Mock<IAzdoRestClient>();

        // Act
        var function = new RepositoryService(client.Object);
        var result = await function.GetUrlAsync(Organization, _fixture.Create<Project>(), repository);

        // Assert
        result.Should().Be(new Uri("https://dev.azure.com/TestOrganization/ProjectId/_git/RepoId"));
    }

    [Fact]
    public async Task ShouldRetrieveCorrectProjectIdForRepositoriesLinkedToBuildPipelines()
    {
        // Arrange
        var repository = _fixture.Build<Repository>()
            .With(r => r.Url, new Uri($"https://dev.azure.com/TestOrganization/ProjectName/_git/RepoName"))
            .Without(r => r.Project)
            .Create();

        var project = _fixture.Build<Project>()
            .With(p => p.Id, "ProjectId")
            .With(p => p.Name, "ProjectName")
            .Create();

        // Act
        var function = new RepositoryService(null);
        var result = await function.GetProjectIdByNameAsync(Organization, project, repository);

        // Assert
        result.Should().Be("ProjectId");
    }
}
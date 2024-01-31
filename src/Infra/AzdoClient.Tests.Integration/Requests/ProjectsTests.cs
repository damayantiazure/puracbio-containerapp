using Castle.Core.Internal;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

[Trait("category", "integration")]
public class ProjectsTests : IClassFixture<TestConfig>
{
    private readonly IAzdoRestClient _client;
    private readonly TestConfig _config;

    public ProjectsTests(TestConfig config)
    {
        _client = new AzdoRestClient(config.Organization, config.Token);
        _config = config;
    }

    /// <summary>
    /// Test if all projects have a Name
    /// </summary>
    [Fact]
    public async Task QueryProject()
    {
        var project = await _client.GetAsync(Project.ProjectById(_config.ProjectName, true));
        project.ShouldNotBeNull();
        project.Id.ShouldNotBeNull();
        project.Name.ShouldNotBeNull();
        project.Capabilities.ShouldNotBeNull();
        project.Capabilities.ProcessTemplate.ShouldNotBeNull();
        project.Capabilities.ProcessTemplate.TemplateTypeId.ShouldNotBeNull();
    }

    [Fact]
    public async Task QueryProjects()
    {
        var definitions = await _client.GetAsync(Project.Projects());
        definitions.ShouldAllBe(_ => !string.IsNullOrEmpty(_.Name));
    }

    [Fact]
    public async Task QueryProjectProperties()
    {
        var projects = await _client.GetAsync(Project.Projects());
        var firstProjectName = projects.First().Name;

        var id = await _client.GetAsync(Project.Properties(firstProjectName));
        id.ShouldNotBeNull();
    }

    [Fact]
    public async Task QuerySingleProjectWithNameShouldReturnAProject()
    {
        var project = await _client.GetAsync(Project.ProjectByName("TAS"));
        project.ShouldNotBeNull();
        project.Name.ShouldBe("TAS");
    }

    [Fact]
    public async Task CreateAndRemoveProject()
    {
        // Arrange
        var projectName = "azdo-client-integration-test-project";
        var projectDescription = "test";
        var projectBody = new Response.Project()
        {
            Name = projectName,
            Description = projectDescription,
            Capabilities = new Response.ProjectCapabilities()
            {
                Versioncontrol = new Response.ProjectVersionControl()
                {
                    SourceControlType = "Git"
                },
                ProcessTemplate = new Response.ProjectProcessTemplate()
                {
                    TemplateTypeId = "6b724908-ef14-45cf-84f8-768b5384da45"
                }
            }
        };

        try
        {
            // Reset State
            await DeleteProjectIfExists(projectName);

            // Assert before
            var project = await _client.GetAsync(Project.ProjectByName(projectName));
            project.ShouldBeNull();

            // Act
            var operationReference = await _client.PostAsync(Project.CreateProject(),
                projectBody, _config.Organization);

            bool isProjectCreationCompleted = false;
            int elapsedTime = 0;

            while (!isProjectCreationCompleted && elapsedTime < 120)
            {
                operationReference = await _client.GetAsync(Project.Operation(
                    operationReference.Id), _config.Organization);
                isProjectCreationCompleted = operationReference.Status == Response.OperationStatus.succeeded;
                if (!isProjectCreationCompleted)
                {
                    Thread.Sleep(3000);
                    elapsedTime += 3;
                }
            }

            // Assert
            project = await _client.GetAsync(Project.ProjectByName(projectName));
            project.ShouldNotBeNull();
            project.Name.ShouldBe(projectName);
            project.Description.ShouldBe(projectDescription);
        }
        finally
        {
            // Clean up
            await DeleteProjectIfExists(projectName);
        }
    }

    private async Task DeleteProjectIfExists(string projectName)
    {
        var projectToDelete = await _client.GetAsync(Project.ProjectByName(projectName), _config.Organization);
        if (projectToDelete != null)
        {
            await _client.DeleteAsync(Project.ProjectById(projectToDelete.Id), _config.Organization);
        }
    }
}
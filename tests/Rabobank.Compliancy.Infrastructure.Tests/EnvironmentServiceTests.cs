using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Tests.FixtureCustomizations;
using Rabobank.Compliancy.Infrastructure.InternalServices;

namespace Rabobank.Compliancy.Infrastructure.Tests;

public class EnvironmentServiceTests
{
    private readonly Mock<ICheckConfigurationRepository> _checkConfigurationRepository = new();
    private readonly Mock<IEnvironmentRepository> _environmentRepository = new();
    private readonly IFixture _fixture = new Fixture();

    public EnvironmentServiceTests()
    {
        _fixture.Customize(new ProjectWithoutPermissions());
    }

    [Fact]
    public async Task GetGatesForBuildDefinitionAsync_WithCorrectIdInput_ReturnsExpectedResult()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var environmentInstance1 = new EnvironmentInstance { Id = 1, Name = "name1" };
        var environmentInstance2 = new EnvironmentInstance { Id = 2, Name = "name2" };
        var checkConfiguration = new CheckConfiguration
        {
            Settings = new CheckSettings
            {
                Inputs = new SettingsInputs
                    { Function = "testfunction", Method = "testmethod", WaitForCompletion = false }
            }
        };

        _environmentRepository.Setup(e =>
                e.GetEnvironmentsAsync(project.Organization, project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { environmentInstance1, environmentInstance2 });
        _checkConfigurationRepository.SetupSequence(c =>
                c.GetCheckConfigurationsForEnvironmentAsync(project.Organization, project.Id, 1,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { checkConfiguration })
            .ReturnsAsync((IEnumerable<CheckConfiguration>?)null);

        // Act
        var actual =
            await new GateService(_environmentRepository.Object, _checkConfigurationRepository.Object)
                .GetGatesForBuildDefinitionAsync(project, 1,
                    new[] { environmentInstance1.Name, environmentInstance2.Name });

        // Assert
        actual.Should().NotBeNull();
        actual.First().Checks.Should().HaveCount(1);
        actual.First().Checks.First().Function.Should().Be(checkConfiguration.Settings.Inputs.Function);
        actual.First().Checks.First().IsEnabled.Should().Be(true);
        actual.First().Checks.First().Method.Should().Be(checkConfiguration.Settings.Inputs.Method);
        actual.First().Checks.First().WaitForCompletion.Should()
            .Be(checkConfiguration.Settings.Inputs.WaitForCompletion == true);
    }

    [Fact]
    public async Task GetGatesForBuildDefinitionAsync_WhenCalledTwice_UsesCachedResults()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var environmentInstance = new EnvironmentInstance { Id = 1, Name = "name" };

        _environmentRepository.Setup(e =>
                e.GetEnvironmentsAsync(project.Organization, project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { environmentInstance });

        // Act
        var sut = new GateService(_environmentRepository.Object, _checkConfigurationRepository.Object);
        var actual1 = await sut.GetGatesForBuildDefinitionAsync(project, 1, new[] { environmentInstance.Name });
        var actual2 = await sut.GetGatesForBuildDefinitionAsync(project, 1, new[] { environmentInstance.Name });

        // Assert
        actual1.Should().NotBeNull();
        actual2.Should().NotBeNull();
        _environmentRepository.Verify(
            e => e.GetEnvironmentsAsync(project.Organization, project.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
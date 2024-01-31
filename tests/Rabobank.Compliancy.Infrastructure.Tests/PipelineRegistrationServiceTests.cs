using Rabobank.Compliancy.Clients.AzureDataTablesClient.DeploymentMethods;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Registrations;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Domain.Tests.FixtureCustomizations;
using Rabobank.Compliancy.Tests;
using TableStorage.Abstractions.Store;

namespace Rabobank.Compliancy.Infrastructure.Tests;

public class PipelineRegistrationServiceTests : UnitTestBase
{
    private const string _yamlReleaseRegistration = "YAML release";
    private const string _classicReleaseRegistration = "Classic release";
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<ITableStore<DeploymentMethodEntity>> _repository = new();

    public PipelineRegistrationServiceTests() =>
        _fixture.Customize(new IdentityIsAlwaysUser());

    [Fact]
    public async Task GetNonProdPipelineRegistrationsForPipelineAsync_WithSuppliedParameters_GeneratesCorrectRowKey()
    {
        // Arrange
        var ciIdentifier = _fixture.Create<string>();
        var expectedRegistration = CreateExpectedNonProdPipelineRegistration(Guid.NewGuid());
        var registrationEntity = ToDeploymentMethodEntity(expectedRegistration, ciIdentifier);
        _repository.Setup(repository =>
            repository.GetRecordsByFilterAsync(It.IsAny<Func<DeploymentMethodEntity, bool>>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new[] { registrationEntity });

        // Act
        var resultingObject = (await new PipelineRegistrationService(() => _repository.Object)
            .GetNonProdPipelineRegistrationsForPipelineAsync(expectedRegistration.Pipeline)).FirstOrDefault();

        // Assert
        AssertRegistrationEquality(expectedRegistration, resultingObject);
    }

    [Fact]
    public async Task GetNonProdPipelineRegistrationsForProjectAsync_WithSuppliedParameters_GeneratesCorrectRowKey()
    {
        // Arrange
        var ciIdentifier = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var expectedRegistration = CreateExpectedNonProdPipelineRegistration(project.Id);
        var registrationEntity = ToDeploymentMethodEntity(expectedRegistration, ciIdentifier);
        _repository.Setup(repository =>
            repository.GetRecordsByFilterAsync(It.IsAny<Func<DeploymentMethodEntity, bool>>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new[] { registrationEntity });

        // Act
        var resultingObject = (await new PipelineRegistrationService(() => _repository.Object)
            .GetNonProdPipelineRegistrationsForProjectAsync(project)).FirstOrDefault();

        // Assert
        AssertRegistrationEquality(expectedRegistration, resultingObject);
    }

    private NonProdPipelineRegistration CreateExpectedNonProdPipelineRegistration(Guid projectId) =>
        new()
        {
            Pipeline = new Pipeline
            {
                DefinitionType = PipelineProcessType.Yaml,
                Id = _fixture.Create<int>(),
                Name = _fixture.Create<string>(),
                Project = _fixture.Build<Project>().With(p => p.Id, projectId).Create()
            },
            RuleProfile = new DefaultRuleProfile(),
            ShouldBeScanned = _fixture.Create<bool>(),
            StageId = _fixture.Create<string>()
        };

    private static void AssertRegistrationEquality(NonProdPipelineRegistration expectedRegistration,
        NonProdPipelineRegistration? resultingObject)
    {
        resultingObject.Should().NotBeNull();
        resultingObject!.Pipeline.DefinitionType.Should().Be(expectedRegistration.Pipeline.DefinitionType);
        resultingObject.Pipeline.Id.Should().Be(expectedRegistration.Pipeline.Id);
        resultingObject.Pipeline.Project.Id.Should().Be(expectedRegistration.Pipeline.Project.Id);
        resultingObject.RuleProfile.Should().Be(expectedRegistration.RuleProfile);
        resultingObject.ShouldBeScanned.Should().Be(expectedRegistration.ShouldBeScanned);
        resultingObject.StageId.Should().Be(expectedRegistration.StageId);
    }

    public static DeploymentMethodEntity ToDeploymentMethodEntity(NonProdPipelineRegistration registration,
        string ciIdentifier)
    {
        var pipeline = registration.Pipeline;
        var deploymentEntity = new DeploymentMethodEntity(pipeline.Project.Organization, ciIdentifier,
            pipeline.Project.Id,
            pipeline.Id, ToRegistrationDefinitionType(pipeline.DefinitionType), registration.StageId)
        {
            RuleProfileName = registration.RuleProfile.Name,
            ToBeScanned = registration.ShouldBeScanned
        };

        return deploymentEntity;
    }

    private static string ToRegistrationDefinitionType(PipelineProcessType processType) =>
        processType switch
        {
            PipelineProcessType.Yaml => _yamlReleaseRegistration,
            PipelineProcessType.DesignerRelease => _classicReleaseRegistration,
            _ => throw new ArgumentException("ProcessType {processType} cannot be mapped to definitionType.")
        };
}
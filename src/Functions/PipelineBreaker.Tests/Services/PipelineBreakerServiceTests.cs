#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Core.PipelineResources.Services;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Functions.PipelineBreaker.Services;
using Rabobank.Compliancy.Functions.PipelineBreaker.Tests.Resources;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;
using Shouldly;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Constants = Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using ErrorMessages = Rabobank.Compliancy.Functions.PipelineBreaker.Exceptions.ErrorMessages;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.PipelineBreaker.Tests.Services;

public class PipelineBreakerServiceTests
{
    private readonly Mock<IAzdoRestClient> _azdoClientMock = new();
    private readonly Mock<IBuildPipelineRule> _buildPipelineRuleMock = new();
    private readonly IEnumerable<IBuildPipelineRule> _buildPipelineRules;
    private readonly Mock<IBuildPipelineService> _buildPipelineServiceMock = new();
    private readonly Mock<IClassicReleasePipelineRule> _classicReleasePipelineRuleMock = new();
    private readonly IEnumerable<IClassicReleasePipelineRule> _classicReleasePipelineRules;
    private readonly ComplianceConfig _complianceConfig;
    private readonly Mock<IDeviationStorageRepository> _deviationRepoMock = new();
    private readonly Mock<IExtensionDataRepository> _extensionDataRepositoryMock = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<ILogQueryService> _logQueryServiceMock = new();
    private readonly Mock<IProjectRule> _projectRuleMock = new();
    private readonly IEnumerable<IProjectRule> _projectRules;
    private readonly Mock<IReleasePipelineService> _releasePipelineServiceMock = new();
    private readonly Mock<IRepositoryRule> _repositoryRuleMock = new();
    private readonly IEnumerable<IRepositoryRule> _repositoryRules;
    private readonly Mock<IRepositoryService> _repositoryServiceMock = new();
    private readonly Mock<IYamlReleasePipelineRule> _yamlReleasePipelineRuleMock = new();
    private readonly IEnumerable<IYamlReleasePipelineRule> _yamlReleasePipelineRules;

    public PipelineBreakerServiceTests()
    {
        _complianceConfig = _fixture.Create<ComplianceConfig>();
        _projectRules = new[] { _projectRuleMock.Object };
        _yamlReleasePipelineRules = new[] { _yamlReleasePipelineRuleMock.Object };
        _classicReleasePipelineRules = new[] { _classicReleasePipelineRuleMock.Object };
        _buildPipelineRules = new[] { _buildPipelineRuleMock.Object };
        _repositoryRules = new[] { _repositoryRuleMock.Object };
    }

    [Fact]
    public async Task GetPreviousRegistrationResultAsync_DataNotFound_ShouldReturnPipelineBreakerResult_None()
    {
        // Arrange
        var pipelineRunInfo = _fixture.Create<PipelineRunInfo>();

        _logQueryServiceMock
            .Setup(c => c.GetQueryEntryAsync<PipelineBreakerRegistrationReport>(It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PipelineBreakerRegistrationReport?)null);

        var sut = CreateSut();

        // Act
        var actual = await sut.GetPreviousRegistrationResultAsync(pipelineRunInfo);

        // Assert
        actual!.Result.ShouldBe(PipelineBreakerResult.None);
    }

    [Fact]
    public async Task GetPreviousRegistrationResultAsync_WithDataFound_ShouldReturnValidObject()
    {
        // Arrange
        var pipelineRunInfo = _fixture.Create<PipelineRunInfo>();
        var expected = _fixture.Create<PipelineBreakerRegistrationReport>();

        _logQueryServiceMock
            .Setup(c => c.GetQueryEntryAsync<PipelineBreakerRegistrationReport>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = CreateSut();

        // Act
        var actual = await sut.GetPreviousRegistrationResultAsync(pipelineRunInfo);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetPreviousComplianceResultAsync_DataNotFound_ShouldReturnPipelineBreakerResult_None()
    {
        // Arrange
        var pipelineRunInfo = _fixture.Create<PipelineRunInfo>();

        _logQueryServiceMock
            .Setup(c => c.GetQueryEntryAsync<PipelineBreakerReport>(It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PipelineBreakerReport?)null);

        var sut = CreateSut();

        // Act
        var actual = await sut.GetPreviousComplianceResultAsync(pipelineRunInfo);

        // Assert
        actual!.Result.ShouldBe(PipelineBreakerResult.None);
    }

    [Fact]
    public async Task GetPreviousComplianceResultAsync_WithDataFound_ShouldReturnValidObject()
    {
        // Arrange
        var pipelineRunInfo = _fixture.Create<PipelineRunInfo>();
        var expected = _fixture.Create<PipelineBreakerReport>();

        _logQueryServiceMock
            .Setup(c => c.GetQueryEntryAsync<PipelineBreakerReport>(It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = CreateSut();

        // Act
        var actual = await sut.GetPreviousComplianceResultAsync(pipelineRunInfo);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task EnrichPipelineInfoAsync_ItemTypeReleasePipeline_GetReleaseInfoAsync_ReturnsReleaseInfo()
    {
        // Arrange
        const string organization = "raboweb-test";
        const string projectId = "1";
        const string runId = "2";
        const string stageId = "1";
        const string pipelineType = ItemTypes.ReleasePipeline;
        const string projectName = "TAS";
        const string releaseDefinitionId = "23";
        const string releaseDefinitionName = "releaseDefinitionName";
        const int environmentId = 1;
        const string environmentName = "stage";
        var environment = _fixture.Build<ReleaseDefinitionEnvironment>()
            .With(e => e.Id, environmentId)
            .With(e => e.Name, environmentName)
            .CreateMany(1)
            .ToList();
        const string revision = "revision";
        const int releaseId = 33;
        const string absoluteUri = "http://url/";

        _azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Project>>(), organization))
            .ReturnsAsync(new Project { Description = "", Id = projectId, Name = projectName });

        _azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Release>>(), organization))
            .ReturnsAsync(new Release
            {
                Links = new Links { Web = new Link { Href = new Uri(absoluteUri) } },
                Id = releaseId,
                ReleaseDefinition = new ReleaseDefinition { Id = releaseDefinitionId },
                ReleaseDefinitionRevision = revision
            });

        _azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<ReleaseDefinition>>(), organization))
            .ReturnsAsync(new ReleaseDefinition
            {
                Id = releaseDefinitionId,
                Name = releaseDefinitionName,
                Environments = environment
            });

        var sut = CreateSut();

        // Act
        var actual =
            await sut.EnrichPipelineInfoAsync(new PipelineRunInfo(organization, projectId, runId, stageId,
                pipelineType));

        // Assert
        actual.Organization.ShouldBe(organization);
        actual.ProjectId.ShouldBe(projectId);
        actual.ProjectName.ShouldBe(projectName);
        actual.PipelineId.ShouldBe(releaseDefinitionId);
        actual.PipelineName.ShouldBe(releaseDefinitionName);
        actual.Stages![0].Id.ShouldBe(environmentId.ToString());
        actual.Stages[0].Name.ShouldBe(environmentName);
        actual.PipelineType.ShouldBe(ItemTypes.ClassicReleasePipeline);
        actual.PipelineVersion.ShouldBe(revision);
        actual.RunId.ShouldBe(releaseId.ToString());
        actual.RunUrl.ShouldBe(absoluteUri);
        actual.StageId.ShouldBe(stageId);
    }

    [Fact]
    public async Task
        EnrichPipelineInfoAsync_ItemTypeNotReleasePipeline_GetBuildInfoAsync_ReturnsClassicBuildPipelineInfo()
    {
        // Arrange
        const string organization = "raboweb-test";
        const string projectId = "1";
        const string runId = "2";
        const string stageId = "1";
        const string pipelineType = ItemTypes.BuildPipeline;
        const string projectName = "TAS";
        const string buildDefinitionId = "23";
        const string revision = "revision";
        const string buildDefinitionName = "releaseDefinitionName";
        const int pipelineProcessType = Constants.PipelineProcessType.GuiPipeline;
        const int buildId = 33;
        const string absoluteUri = "http://url/";

        _azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Build>>(), organization))
            .ReturnsAsync(new Build
            {
                Id = buildId,
                Definition = new Definition { Revision = revision },
                Links = new Links { Web = new Link { Href = new Uri(absoluteUri) } }
            });

        _azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), organization))
            .ReturnsAsync(new BuildDefinition
            {
                Process = new BuildProcess { Type = pipelineProcessType },
                Id = buildDefinitionId,
                Name = buildDefinitionName,
                Project = new TeamProjectReference { Id = projectId, Name = projectName }
            });

        _azdoClientMock
            .Setup(c => c.GetAsStringAsync(It.IsAny<IAzdoRequest>(), It.IsAny<string>()))
            .ReturnsAsync("");

        var sut = CreateSut();

        // Act
        var actual =
            await sut.EnrichPipelineInfoAsync(new PipelineRunInfo(organization, projectId, runId, stageId,
                pipelineType));

        // Assert
        actual.Organization.ShouldBe(organization);
        actual.ProjectId.ShouldBe(projectId);
        actual.ProjectName.ShouldBe(projectName);
        actual.PipelineId.ShouldBe(buildDefinitionId);
        actual.PipelineName.ShouldBe(buildDefinitionName);
        actual.Stages.ShouldBeEmpty();
        actual.PipelineType.ShouldBe(ItemTypes.ClassicBuildPipeline);
        actual.PipelineVersion.ShouldBe(revision);
        actual.RunId.ShouldBe(buildId.ToString());
        actual.RunUrl.ShouldBe(absoluteUri);
        actual.StageId.ShouldBe(stageId);
    }

    [Fact]
    public async Task
        EnrichPipelineInfoAsync_ItemTypeNotReleasePipeline_GetBuildInfoAsync_ReturnsStagelessYamlPipelineInfo()
    {
        // Arrange
        const string organization = "raboweb-test";
        const string projectId = "1";
        const string runId = "2";
        const string stageId = "1";
        const string pipelineType = ItemTypes.BuildPipeline;
        const string projectName = "TAS";
        const string buildDefinitionId = "23";
        const string revision = "revision";
        const string sourceBranch = "develop";
        const string buildDefinitionName = "releaseDefinitionName";
        const int pipelineProcessType = Constants.PipelineProcessType.YamlPipeline;
        const int buildId = 33;
        const string absoluteUri = "http://url/";

        _azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Build>>(), organization))
            .ReturnsAsync(new Build
            {
                Id = buildId,
                Definition = new Definition { Revision = revision },
                Links = new Links { Web = new Link { Href = new Uri(absoluteUri) } },
                Project = new Project { Name = projectName },
                SourceBranch = sourceBranch
            });

        _azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), organization))
            .ReturnsAsync(new BuildDefinition
            {
                Process = new BuildProcess { Type = pipelineProcessType },
                Id = buildDefinitionId,
                Name = buildDefinitionName,
                Project = new TeamProjectReference { Id = projectId, Name = projectName }
            });

        _azdoClientMock
            .Setup(c => c.GetAsStringAsync(It.IsAny<IAzdoRequest>(), It.IsAny<string>()))
            .ReturnsAsync(DummyYamlResponses.StagelessYamlPipeline);

        var sut = CreateSut();

        // Act
        var actual =
            await sut.EnrichPipelineInfoAsync(new PipelineRunInfo(organization, projectId, runId, stageId,
                pipelineType));

        // Assert
        actual.Organization.ShouldBe(organization);
        actual.ProjectId.ShouldBe(projectId);
        actual.ProjectName.ShouldBe(projectName);
        actual.PipelineId.ShouldBe(buildDefinitionId);
        actual.PipelineName.ShouldBe(buildDefinitionName);
        actual.Stages![0].Id.ShouldBe("__default");
        actual.PipelineType.ShouldBe(ItemTypes.StagelessYamlPipeline);
        actual.PipelineVersion.ShouldBe(revision);
        actual.RunId.ShouldBe(buildId.ToString());
        actual.RunUrl.ShouldBe(absoluteUri);
        actual.StageId.ShouldBe(stageId);
    }

    [Fact]
    public async Task
        EnrichPipelineInfoAsync_ItemTypeNotReleasePipeline_GetBuildInfoAsync_DefaultIsStagelessYaml_CurrentIsYamlWithStages_ReturnsAsYamlPipelineWithStages()
    {
        // Arrange
        const string organization = "raboweb-test";
        const string projectId = "1";
        const string projectName = "TAS";
        const string buildDefinitionId = "23";
        const string buildDefinitionName = "buildDefinitionName";
        const string absoluteUri = "http://url/";

        _azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Build>>(), organization))
            .ReturnsAsync(new Build
            {
                Id = 33,
                Definition = new Definition { Revision = "revision" },
                Links = new Links { Web = new Link { Href = new Uri(absoluteUri) } },
                Project = new Project { Name = projectName },
                SourceBranch = "MyBranch"
            });

        // PipelineProcessType is a YAML type pipeline
        _azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), organization))
            .ReturnsAsync(new BuildDefinition
            {
                Process = new BuildProcess { Type = Constants.PipelineProcessType.YamlPipeline },
                Id = buildDefinitionId,
                Name = buildDefinitionName,
                Project = new TeamProjectReference { Id = projectId, Name = projectName }
            });

        // Default Branch is StagelessYamlPipeline
        var resourceReportDto = new ResourceReportDto
        {
            Id = buildDefinitionId,
            Name = buildDefinitionName,
            Type = ItemTypes.StagelessYamlPipeline,
            Link = absoluteUri
        };

        var complianceReportDto = _fixture.Build<CompliancyReportDto>()
            .With(f => f.BuildPipelines, new[] { resourceReportDto })
            .Create();

        _extensionDataRepositoryMock.Setup(m =>
                m.DownloadAsync<CompliancyReportDto>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(complianceReportDto);

        // Current Run is YamlPipelineWithStages - Single Stage
        _azdoClientMock
            .Setup(c => c.GetAsStringAsync(It.IsAny<IAzdoRequest>(), It.IsAny<string>()))
            .ReturnsAsync(DummyYamlResponses.YamlPipelineWithStagesSingleStage);

        var sut = CreateSut();

        // Act
        // EnrichPipelineInfoAsync for a Non-ReleasePipeline
        var actual = await sut.EnrichPipelineInfoAsync(new PipelineRunInfo(organization, projectId, "dummy",
            "dummy", ItemTypes.BuildPipeline));

        // Assert
        actual.PipelineType.ShouldBe(ItemTypes.YamlPipelineWithStages);
    }

    [Fact]
    public async Task EnrichPipelineInfoAsync_BuildNotFoundOrIncorrectPermissions_ThrowsExceptionWithCorrectMessage()
    {
        // Arrange
        const string organization = "raboweb-test";
        const string runId = "2";

        _azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), organization))!
            .ReturnsAsync((BuildDefinition?)null);

        var sut = CreateSut();

        // Act & Assert            
        await sut.EnrichPipelineInfoAsync(new PipelineRunInfo(organization, "1", runId, "dummy",
            ItemTypes.BuildPipeline)).ShouldThrowAsync(typeof(InvalidOperationException),
            ErrorMessages.BuildNotAvailableErrorMessage(runId));
    }

    [Fact]
    public async Task EnrichPipelineInfoAsync_ReleaseNotFoundOrIncorrectPermissions_ThrowsExceptionWithCorrectMessage()
    {
        // Arrange
        const string organization = "raboweb-test";
        const string releaseId = "2";

        _azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(), organization))!
            .ReturnsAsync((BuildDefinition?)null);

        var sut = CreateSut();

        // Act & Assert            
        await sut.EnrichPipelineInfoAsync(new PipelineRunInfo(organization, "1", releaseId, "dummy",
            ItemTypes.ReleasePipeline)).ShouldThrowAsync(typeof(InvalidOperationException),
            ErrorMessages.ReleaseNotAvailableErrorMessage(releaseId));
    }

    [Fact]
    public async Task GetCompliancyAsync_NoProdStages_ReturnsEmptyList()
    {
        // Arrange
        var registrations = _fixture.CreateMany<PipelineRegistration>();
        var pipelineRunInfo = _fixture.Create<PipelineRunInfo>();

        var sut = CreateSut();

        // Act
        var actual = await sut.GetCompliancy(pipelineRunInfo, registrations);

        // Assert
        actual.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetCompliancyAsync_ClassicPipeline_ShouldEvaluateCorrectRules()
    {
        // Arrange
        var registrations = _fixture.Build<PipelineRegistration>()
            .With(p => p.PartitionKey, PipelineRegistration.Prod)
            .With(r => r.StageId, "prod")
            .CreateMany(1)
            .ToList();

        var runInfo = new PipelineRunInfo("", "", "", "prod", ItemTypes.ClassicReleasePipeline)
        {
            ClassicReleasePipeline = _fixture.Create<ReleaseDefinition>(),
            BuildPipeline = _fixture.Create<BuildDefinition>(),
            Stages = new List<StageReport> { new() { Id = "prod" } }
        };

        SetUpTestEnvironment();

        _classicReleasePipelineRuleMock
            .Setup(m => m.EvaluateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReleaseDefinition>()))
            .ReturnsAsync(false);

        _projectRuleMock
            .Setup(m => m.EvaluateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _releasePipelineServiceMock
            .Setup(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<ReleaseDefinition>(), It.IsAny<string>(),
                null))
            .ReturnsAsync(new List<BuildDefinition>());

        _releasePipelineServiceMock
            .Setup(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<IList<ReleaseDefinition>>(),
                It.IsAny<IList<BuildDefinition>>()))
            .ReturnsAsync(new List<Repository>());

        var sut = CreateSut();

        // Act
        var actual = (await sut.GetCompliancy(runInfo, registrations)).ToList();

        // Assert
        actual.Count.ShouldBe(2);
        var retentionRuleReport = actual.Single(r => r.RuleDescription == "All releases are retained");
        retentionRuleReport.HasDeviation.ShouldBeFalse();
        retentionRuleReport.IsCompliant.ShouldBeFalse();
        var projectRuleReport = actual.Single(r => r.RuleDescription == "Nobody can delete the project");
        projectRuleReport.HasDeviation.ShouldBeFalse();
        projectRuleReport.IsCompliant.ShouldBeTrue();
    }

    [Fact]
    public async Task GetCompliancyAsync_YamlPipeline_ShouldEvaluateCorrectRules()
    {
        // Arrange
        var registrations = _fixture.Build<PipelineRegistration>()
            .With(p => p.PartitionKey, PipelineRegistration.Prod)
            .With(r => r.StageId, "prod")
            .CreateMany(1)
            .ToList();

        var runInfo = new PipelineRunInfo("", "", "", "prod", ItemTypes.YamlReleasePipeline)
        {
            BuildPipeline = _fixture.Create<BuildDefinition>(),
            Stages = new List<StageReport> { new() { Id = "prod" } }
        };

        SetUpTestEnvironment();

        _yamlReleasePipelineRuleMock.Setup(m =>
                m.EvaluateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BuildDefinition>()))
            .ReturnsAsync(false);

        _projectRuleMock
            .Setup(m => m.EvaluateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _buildPipelineServiceMock
            .Setup(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<BuildDefinition>(), null))
            .ReturnsAsync(_fixture.CreateMany<BuildDefinition>(0).ToList());

        _buildPipelineServiceMock
            .Setup(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<List<BuildDefinition>>()))
            .ReturnsAsync(_fixture.CreateMany<Repository>(0).ToList());

        var sut = CreateSut();

        // Act
        var actual = (await sut.GetCompliancy(runInfo, registrations)).ToList();

        // Assert
        actual.Count.ShouldBe(2);
        var projectRuleReport = actual.Single(r => r.RuleDescription == "Nobody can delete the project");
        projectRuleReport.HasDeviation.ShouldBeFalse();
        projectRuleReport.IsCompliant.ShouldBeTrue();
        var buildRuleReport = actual.Single(r => r.RuleDescription == "Nobody can delete builds");
        buildRuleReport.HasDeviation.ShouldBeFalse();
        buildRuleReport.IsCompliant.ShouldBeFalse();
    }

    [Fact]
    public async Task GetCompliancyAsync_ClassicPipeline_ShouldReturnCorrectLinkedResources()
    {
        // Arrange
        var registrations = _fixture.Build<PipelineRegistration>()
            .With(p => p.PartitionKey, PipelineRegistration.Prod)
            .With(r => r.StageId, "prod")
            .CreateMany(1)
            .ToList();

        var runInfo = new PipelineRunInfo("", "projectA", "", "prod", ItemTypes.ClassicReleasePipeline)
        {
            BuildPipeline = _fixture.Create<BuildDefinition>(),
            ClassicReleasePipeline = _fixture.Create<ReleaseDefinition>(),
            Stages = new List<StageReport> { new() { Id = "prod" } }
        };

        var buildPipeline = _fixture.Create<BuildDefinition>();
        buildPipeline.Project.Id = "projectB";
        buildPipeline.Name = "itemname";

        var repository = _fixture.Create<Repository>();
        repository.Name = "repositoryName";

        SetUpTestEnvironment();

        _releasePipelineServiceMock
            .Setup(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<ReleaseDefinition>(), It.IsAny<string>(),
                null))
            .ReturnsAsync(new List<BuildDefinition> { buildPipeline });

        _releasePipelineServiceMock
            .Setup(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<IList<ReleaseDefinition>>(),
                It.IsAny<IList<BuildDefinition>>()))
            .ReturnsAsync(new List<Repository> { repository });

        var sut = CreateSut();

        // Act
        var actual = (await sut.GetCompliancy(runInfo, registrations)).ToList();

        // Assert
        actual.ShouldContain(x => x.ItemName == buildPipeline.Name);
        actual.ShouldContain(r => r.ItemName == repository.Name);
    }

    [Theory]
    [InlineData(ItemTypes.ClassicReleasePipeline)]
    [InlineData(ItemTypes.YamlReleasePipeline)]
    public async Task
        GetCompliancyAsync_YamlAndClassicPipeline_WithDeviation_ForLinkedBuildPipelineFromOtherProject_ShouldReturnCorrectDeviation(
            string pipelineType)
    {
        // Arrange
        var registrations = _fixture.Build<PipelineRegistration>()
            .With(r => r.PartitionKey, PipelineRegistration.Prod)
            .With(r => r.StageId, "prod")
            .With(r => r.CiIdentifier, "CI123456")
            .CreateMany(1)
            .ToList();

        var runInfo = new PipelineRunInfo("", "projectA", "", "prod", pipelineType)
        {
            BuildPipeline = _fixture.Create<BuildDefinition>(),
            ClassicReleasePipeline = _fixture.Create<ReleaseDefinition>(),
            Stages = new List<StageReport> { new() { Id = "prod" } }
        };

        var buildPipeline = _fixture.Create<BuildDefinition>();
        buildPipeline.Id = "itemId";
        buildPipeline.Project.Id = "projectB";
        buildPipeline.Name = "itemname";

        var deviation = _fixture.Build<Deviation>()
            .With(d => d.ItemId, "itemId")
            .With(d => d.RuleName, RuleNames.NobodyCanDeleteBuilds)
            .With(d => d.ProjectId, "projectA")
            .With(d => d.ForeignProjectId, "projectB")
            .With(d => d.CiIdentifier, "CI123456")
            .Create();

        SetUpTestEnvironment();

        _classicReleasePipelineRuleMock
            .Setup(m => m.EvaluateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReleaseDefinition>()))
            .ReturnsAsync(false);

        _yamlReleasePipelineRuleMock
            .Setup(m => m.EvaluateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BuildDefinition>()))
            .ReturnsAsync(false);

        _releasePipelineServiceMock
            .Setup(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<ReleaseDefinition>(), It.IsAny<string>(),
                null))
            .ReturnsAsync(new List<BuildDefinition> { buildPipeline });

        _releasePipelineServiceMock
            .Setup(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<IList<ReleaseDefinition>>(),
                It.IsAny<IList<BuildDefinition>>()))
            .ReturnsAsync(Enumerable.Empty<Repository>());

        _buildPipelineServiceMock
            .Setup(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<BuildDefinition>(), null))
            .ReturnsAsync(new List<BuildDefinition> { buildPipeline });

        _buildPipelineServiceMock
            .Setup(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<List<BuildDefinition>>()))
            .ReturnsAsync(Enumerable.Empty<Repository>());

        _deviationRepoMock
            .Setup(d => d.GetListAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Deviation> { deviation });

        var sut = CreateSut();

        // Act
        var actual = (await sut.GetCompliancy(runInfo, registrations)).ToList();

        // Assert
        actual.Count.ShouldBe(3);
        var buildRuleReport =
            actual.Single(r => r.RuleDescription == "Nobody can delete builds" && r.ItemName == "itemname");
        buildRuleReport.HasDeviation.ShouldBeTrue();
        buildRuleReport.IsCompliant.ShouldBeFalse();
        buildRuleReport.ToString()
            .ShouldBe("Rule: 'Nobody can delete builds' for 'itemname' is compliant with deviation. ");
    }

    [Fact]
    public async Task GetCompliancyAsync_DeviationForSameItemButAnotherCI_ShouldNotReturnDeviation()
    {
        // Arrange
        var registrations = _fixture.Build<PipelineRegistration>()
            .With(r => r.PartitionKey, PipelineRegistration.Prod)
            .With(r => r.StageId, "RegisteredProdStage")
            .With(r => r.CiIdentifier, "CI123456")
            .CreateMany(1)
            .ToList();

        var runInfo = new PipelineRunInfo("", "projectA", "", "RegisteredProdStage", ItemTypes.ClassicReleasePipeline)
        {
            Stages = new List<StageReport> { new() { Id = "RegisteredProdStage" } },
            PipelineId = "itemId",
            ClassicReleasePipeline = _fixture.Build<ReleaseDefinition>()
                .With(r => r.Id, "itemId")
                .With(r => r.Name, "itemName")
                .Create()
        };

        var deviation = _fixture.Build<Deviation>()
            .With(d => d.ItemId, "itemId")
            .With(d => d.RuleName, RuleNames.ClassicReleasePipelineHasRequiredRetentionPolicy)
            .With(d => d.ProjectId, "projectA")
            .With(d => d.CiIdentifier, "CI789654")
            .Create();

        SetUpTestEnvironment();

        _classicReleasePipelineRuleMock
            .Setup(m => m.EvaluateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReleaseDefinition>()))
            .ReturnsAsync(false);

        _releasePipelineServiceMock
            .Setup(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<ReleaseDefinition>(), It.IsAny<string>(),
                null))
            .ReturnsAsync(_fixture.CreateMany<BuildDefinition>(0).ToList());

        _releasePipelineServiceMock
            .Setup(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<IList<ReleaseDefinition>>(),
                It.IsAny<IList<BuildDefinition>>()))
            .ReturnsAsync(_fixture.CreateMany<Repository>(0).ToList());

        _deviationRepoMock
            .Setup(d => d.GetListAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Deviation> { deviation });

        var sut = CreateSut();

        // Act
        var actual = (await sut.GetCompliancy(runInfo, registrations)).ToList();

        // Assert
        actual.Count.ShouldBe(2);
        var classicPipelinesReport = actual.Single(r => r.RuleDescription == "All releases are retained"
                                                        && r.ItemName == "itemName");
        classicPipelinesReport.HasDeviation.ShouldBeFalse();
        classicPipelinesReport.IsCompliant.ShouldBeFalse();
        classicPipelinesReport.ToString().ShouldBe("Rule: 'All releases are retained' for 'itemName' is incompliant. ");
    }

    [Fact]
    public async Task GetCompliancyAsync_ReleasePipelineLinkedToMultipleCIs_ShouldCheckDeviationsForAllCIs()
    {
        // Arrange
        var registrations = new List<PipelineRegistration>
        {
            new()
            {
                PartitionKey = PipelineRegistration.Prod,
                StageId = "Prod",
                CiIdentifier = "CI123456"
            },
            new()
            {
                PartitionKey = PipelineRegistration.Prod,
                StageId = "Prod",
                CiIdentifier = "CI876543"
            }
        };

        var runInfo = new PipelineRunInfo("", "projectA", "", "Prod", ItemTypes.ClassicReleasePipeline)
        {
            Stages = new List<StageReport> { new() { Id = "Prod" } },
            PipelineId = "itemId",
            ClassicReleasePipeline = _fixture.Build<ReleaseDefinition>()
                .With(r => r.Id, "itemId")
                .With(r => r.Name, "itemName")
                .Create()
        };

        var deviation = _fixture.Build<Deviation>()
            .With(d => d.ItemId, "itemId")
            .With(d => d.RuleName, RuleNames.ClassicReleasePipelineHasRequiredRetentionPolicy)
            .With(d => d.ProjectId, "projectA")
            .With(d => d.CiIdentifier, "CI876543")
            .Create();

        SetUpTestEnvironment();

        _classicReleasePipelineRuleMock
            .Setup(m => m.EvaluateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReleaseDefinition>()))
            .ReturnsAsync(false);

        _releasePipelineServiceMock
            .Setup(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<ReleaseDefinition>(), It.IsAny<string>(),
                null))
            .ReturnsAsync(_fixture.CreateMany<BuildDefinition>(0).ToList());

        _releasePipelineServiceMock
            .Setup(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<IList<ReleaseDefinition>>(),
                It.IsAny<IList<BuildDefinition>>()))
            .ReturnsAsync(_fixture.CreateMany<Repository>(0).ToList());

        _deviationRepoMock
            .Setup(d => d.GetListAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Deviation> { deviation });

        var sut = CreateSut();

        // Act
        var actual = (await sut.GetCompliancy(runInfo, registrations)).ToList();

        // Assert
        actual.Count.ShouldBe(2);
        var classicPipelinesReport = actual.Single(r => r.RuleDescription == "All releases are retained"
                                                        && r.ItemName == "itemName");
        classicPipelinesReport.HasDeviation.ShouldBeFalse();
        classicPipelinesReport.IsCompliant.ShouldBeFalse();
        classicPipelinesReport.ToString().ShouldBe("Rule: 'All releases are retained' for 'itemName' is incompliant. ");
    }

    [Fact]
    public async Task GetCompliancyAsync_ProdPipelineRunningBuildStage_ReturnsEmptyList()
    {
        // Arrange
        var registrations = _fixture.Build<PipelineRegistration>()
            .With(r => r.PartitionKey, PipelineRegistration.Prod)
            .With(r => r.StageId, "ProdStage")
            .CreateMany(1)
            .ToList();

        var pipelineRunInfo = _fixture.Build<PipelineRunInfo>()
            .With(p => p.PipelineId, "PipelineId")
            .With(p => p.PipelineType, ItemTypes.ClassicReleasePipeline)
            .With(p => p.StageId, "Build")
            .With(p => p.Stages, new List<StageReport> { new() { Id = "Build" }, new() { Id = "ProdStage" } })
            .Create();

        var sut = CreateSut();

        // Act
        var actual = await sut.GetCompliancy(pipelineRunInfo, registrations);

        // Assert
        actual.ShouldBeEmpty();
    }

    private PipelineBreakerService CreateSut() =>
        new(_azdoClientMock.Object, _logQueryServiceMock.Object, _complianceConfig,
            _projectRules,
            _classicReleasePipelineRules,
            _yamlReleasePipelineRules,
            _buildPipelineRules,
            _repositoryRules,
            _deviationRepoMock.Object,
            _buildPipelineServiceMock.Object,
            _releasePipelineServiceMock.Object,
            _repositoryServiceMock.Object,
            _extensionDataRepositoryMock.Object);

    private void SetUpTestEnvironment()
    {
        _projectRuleMock.SetupGet(m => m.Name).Returns(RuleNames.NobodyCanDeleteTheProject);
        _projectRuleMock.SetupGet(m => m.Description).Returns("Nobody can delete the project");
        _classicReleasePipelineRuleMock.SetupGet(m => m.Name)
            .Returns(RuleNames.ClassicReleasePipelineHasRequiredRetentionPolicy);
        _classicReleasePipelineRuleMock.SetupGet(m => m.Description).Returns("All releases are retained");
        _yamlReleasePipelineRuleMock.SetupGet(m => m.Name).Returns(RuleNames.NobodyCanDeleteBuilds);
        _yamlReleasePipelineRuleMock.SetupGet(m => m.Description).Returns("Nobody can delete builds");
        _repositoryRuleMock.SetupGet(m => m.Name).Returns(RuleNames.NobodyCanDeleteTheRepository);
        _buildPipelineRuleMock.SetupGet(m => m.Name).Returns(RuleNames.NobodyCanDeleteBuilds);
        _buildPipelineRuleMock.SetupGet(m => m.Description).Returns("Nobody can delete builds");
    }
}
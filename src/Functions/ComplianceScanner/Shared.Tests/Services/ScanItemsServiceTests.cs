using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Core.PipelineResources.Services;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Tests.Services;

public class ScanItemsServiceTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<IProjectRule> _projectRule = new();
    private readonly IEnumerable<IProjectRule> _projectRules;
    private readonly Mock<IRepositoryRule> _repositoryRule = new();
    private readonly IEnumerable<IRepositoryRule> _repositoryRules;
    private readonly Mock<IBuildPipelineRule> _buildPipelineRuleMock = new();
    private readonly IEnumerable<IBuildPipelineRule> _buildPipelineRules;
    private readonly Mock<IYamlReleasePipelineRule> _yamlReleasePipelineRule = new();
    private readonly IEnumerable<IYamlReleasePipelineRule> _yamlReleasePipelineRules;
    private readonly Mock<IClassicReleasePipelineRule> _classicReleasePipelineRule = new();
    private readonly IEnumerable<IClassicReleasePipelineRule> _classicReleasePipelineRules;
    private readonly Mock<IRepositoryService> _repositoryService = new();
    private readonly ComplianceConfig _config;

    public ScanItemsServiceTests()
    {
        _projectRules = new[] { _projectRule.Object };
        _repositoryRules = new[] { _repositoryRule.Object };
        _buildPipelineRules = new[] { _buildPipelineRuleMock.Object };
        _yamlReleasePipelineRules = new[] { _yamlReleasePipelineRule.Object };
        _classicReleasePipelineRules = new[] { _classicReleasePipelineRule.Object };
        _config = _fixture.Create<ComplianceConfig>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ScanProject_ShouldReturn_CorrectResult(bool scanResult)
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var ciIdentifier = _fixture.Create<string>();

        _projectRule
            .Setup(x => x.EvaluateAsync(organization, project.Id))
            .ReturnsAsync(scanResult)
            .Verifiable();

        // Act
        var service = new ScanItemsService(_projectRules, _repositoryRules, _buildPipelineRules,
            _yamlReleasePipelineRules, _classicReleasePipelineRules, _config, _repositoryService.Object);
        var result = await service.ScanProjectAsync(organization, project, ciIdentifier);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(_projectRules.Count());
        var evaluatedRule = result.First();
        evaluatedRule.Status.ShouldBe(scanResult);
        evaluatedRule.Item.Id.ShouldBe(project.Id);
        evaluatedRule.Item.Name.ShouldBe(project.Name);
        _projectRule.Verify();
    }

    [Fact]
    public async Task NoRepositories_ShouldReturn_DummyItem()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var repositories = Enumerable.Empty<Repository>();
        var ciIdentifier = _fixture.Create<string>();

        // Act
        var service = new ScanItemsService(_projectRules, _repositoryRules, _buildPipelineRules,
            _yamlReleasePipelineRules, _classicReleasePipelineRules, _config, _repositoryService.Object);
        var result = await service.ScanRepositoriesAsync(organization, project, repositories, ciIdentifier);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(1);
        var evaluatedRule = result.First();
        evaluatedRule.Status.ShouldBe(false);
        evaluatedRule.Item.Id.ShouldBe(ItemTypes.Dummy);
        evaluatedRule.Item.Type.ShouldBe(ItemTypes.Dummy);
        evaluatedRule.Reconcile.ShouldBeNull();
        evaluatedRule.RescanUrl.ShouldNotBeNull();
        evaluatedRule.RegisterDeviationUrl.ShouldNotBeNull();
        evaluatedRule.DeleteDeviationUrl.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ScanRepositories_ShouldReturn_CorrectResult(bool scanResult)
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Build<Project>()
            .With(p => p.Id, "1")
            .Create();
        var repository = _fixture.Create<Repository>();
        repository.Project.Id = "1";
        var repositories = new[] { repository };
        var ciIdentifier = _fixture.Create<string>();

        _repositoryRule
            .Setup(x => x.EvaluateAsync(organization, repository.Project.Id, repository.Id))
            .ReturnsAsync(scanResult)
            .Verifiable();

        _repositoryService
            .Setup(s => s.GetUrlAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<Repository>()))
            .ReturnsAsync(_fixture.Create<Uri>());

        // Act
        var service = new ScanItemsService(_projectRules, _repositoryRules, _buildPipelineRules,
            _yamlReleasePipelineRules, _classicReleasePipelineRules, _config, _repositoryService.Object);
        var result = await service.ScanRepositoriesAsync(organization, project, repositories, ciIdentifier);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(_repositoryRules.Count());
        var evaluatedRule = result.First();
        evaluatedRule.Status.ShouldBe(scanResult);
        evaluatedRule.Item.Id.ShouldBe(repository.Id);
        evaluatedRule.Item.Name.ShouldBe(repository.Name);
        evaluatedRule.Item.ProjectId.ShouldBe(project.Id);
        evaluatedRule.RescanUrl.ToString().ShouldEndWith("/");
        evaluatedRule.RegisterDeviationUrl.ToString().ShouldEndWith("/");
        evaluatedRule.DeleteDeviationUrl.ToString().ShouldEndWith("/");
        _repositoryRule.Verify();
    }

    [Fact]
    public async Task ScanCrossProjectRepositoryResources_ShouldReturn_CorrectResult()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Build<Project>()
            .With(p => p.Id, "1")
            .Create();
        var repository = _fixture.Create<Repository>();
        repository.Project.Id = "2";
        var repositories = new[] { repository };
        var ciIdentifier = _fixture.Create<string>();

        _repositoryRule
            .Setup(x => x.EvaluateAsync(organization, repository.Project.Id, repository.Id))
            .ReturnsAsync(true)
            .Verifiable();

        _repositoryService
            .Setup(s => s.GetUrlAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<Repository>()))
            .ReturnsAsync(_fixture.Create<Uri>());

        // Act
        var service = new ScanItemsService(_projectRules, _repositoryRules, _buildPipelineRules,
            _yamlReleasePipelineRules, _classicReleasePipelineRules, _config, _repositoryService.Object);
        var result = await service.ScanRepositoriesAsync(organization, project, repositories, ciIdentifier);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(_repositoryRules.Count());
        var evaluatedRule = result.First();
        evaluatedRule.Status.ShouldBe(true);
        evaluatedRule.Item.Id.ShouldBe(repository.Id);
        evaluatedRule.Item.Name.ShouldBe(repository.Name);
        evaluatedRule.Item.ProjectId.ShouldBe(repository.Project.Id);
        evaluatedRule.RescanUrl.ToString().ShouldEndWith(repository.Project.Id);
        evaluatedRule.RegisterDeviationUrl.ToString().ShouldEndWith(repository.Project.Id);
        evaluatedRule.DeleteDeviationUrl.ToString().ShouldEndWith(repository.Project.Id);
        _repositoryRule.Verify();
    }

    [Fact]
    public async Task NoBuildPipelines_ShouldReturn_DummyItem()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var buildPipelines = Enumerable.Empty<BuildDefinition>();
        var ciIdentifier = _fixture.Create<string>();

        var rulesProfiles = new List<RuleProfile>()
        {
            new MainFrameCobolRuleProfile()
        };

        _buildPipelineRuleMock.Setup(m => m.Name)
            .Returns(RuleNames.ClassicReleasePipelineFollowsMainframeCobolReleaseProcess);

        // Act
        var service = new ScanItemsService(_projectRules, _repositoryRules, _buildPipelineRules,
            _yamlReleasePipelineRules, _classicReleasePipelineRules, _config, _repositoryService.Object);
        var result = await service.ScanBuildPipelinesAsync(organization, project, buildPipelines, ciIdentifier,
            rulesProfiles);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(1);
        var evaluatedRule = result.First();
        evaluatedRule.Status.ShouldBe(false);
        evaluatedRule.Item.Id.ShouldBe(ItemTypes.Dummy);
        evaluatedRule.Item.Type.ShouldBe(ItemTypes.Dummy);
        evaluatedRule.Reconcile.ShouldBeNull();
        evaluatedRule.RescanUrl.ShouldNotBeNull();
        evaluatedRule.RegisterDeviationUrl.ShouldNotBeNull();
        evaluatedRule.DeleteDeviationUrl.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ScanBuildPipelines_ShouldReturn_CorrectResult(bool scanResult)
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Build<Project>()
            .With(p => p.Id, "1")
            .Create();
        var buildDefinition = _fixture.Create<BuildDefinition>();
        buildDefinition.Project.Id = "1";
        var buildDefinitions = new[] { buildDefinition };
        var ciIdentifier = _fixture.Create<string>();

        _buildPipelineRuleMock
            .Setup(x => x.EvaluateAsync(organization, buildDefinition.Project.Id, buildDefinition))
            .ReturnsAsync(scanResult)
            .Verifiable();

        _buildPipelineRuleMock.Setup(x => x.Name)
            .Returns(RuleNames.BuildPipelineHasCredScanTask);

        // Act
        var service = new ScanItemsService(_projectRules, _repositoryRules, _buildPipelineRules,
            _yamlReleasePipelineRules, _classicReleasePipelineRules, _config, _repositoryService.Object);
        var result = await service.ScanBuildPipelinesAsync(organization, project, buildDefinitions, ciIdentifier, null);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(_buildPipelineRules.Count());
        var evaluatedRule = result.First();
        evaluatedRule.Status.ShouldBe(scanResult);
        evaluatedRule.Item.Id.ShouldBe(buildDefinition.Id);
        evaluatedRule.Item.Name.ShouldBe(buildDefinition.Name);
        evaluatedRule.Item.ProjectId.ShouldBe(project.Id);
        evaluatedRule.RescanUrl.ToString().ShouldEndWith("/");
        evaluatedRule.RegisterDeviationUrl.ToString().ShouldEndWith("/");
        evaluatedRule.DeleteDeviationUrl.ToString().ShouldEndWith("/");
        _buildPipelineRuleMock.Verify();
    }

    [Fact]
    public async Task ScanCrossProjectBuildPipelineResources_ShouldReturn_CorrectResult()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Build<Project>()
            .With(p => p.Id, "1")
            .Create();
        var buildPipeline = _fixture.Create<BuildDefinition>();
        buildPipeline.Project.Id = "2";
        var buildPipelines = new[] { buildPipeline };
        var ciIdentifier = _fixture.Create<string>();

        _buildPipelineRuleMock
            .Setup(x => x.EvaluateAsync(organization, buildPipeline.Project.Id, buildPipeline))
            .ReturnsAsync(false)
            .Verifiable();

        _buildPipelineRuleMock.Setup(x => x.Name)
            .Returns(RuleNames.BuildPipelineHasCredScanTask);

        // Act
        var service = new ScanItemsService(_projectRules, _repositoryRules, _buildPipelineRules,
            _yamlReleasePipelineRules, _classicReleasePipelineRules, _config, _repositoryService.Object);
        var result = await service.ScanBuildPipelinesAsync(organization, project, buildPipelines, ciIdentifier, null);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(_buildPipelineRules.Count());
        var evaluatedRule = result.First();
        evaluatedRule.Status.ShouldBe(false);
        evaluatedRule.Item.Id.ShouldBe(buildPipeline.Id);
        evaluatedRule.Item.Name.ShouldBe(buildPipeline.Name);
        evaluatedRule.Item.ProjectId.ShouldBe(buildPipeline.Project.Id);
        evaluatedRule.RescanUrl.ToString().ShouldEndWith(buildPipeline.Project.Id);
        evaluatedRule.RegisterDeviationUrl.ToString().ShouldEndWith(buildPipeline.Project.Id);
        evaluatedRule.DeleteDeviationUrl.ToString().ShouldEndWith(buildPipeline.Project.Id);
        _buildPipelineRuleMock.Verify();
    }

    [Fact]
    public async Task NoYamlPipelines_ShouldReturn_NoRules()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var ciIdentifier = _fixture.Create<string>();
        var yamlPipelines = Enumerable.Empty<BuildDefinition>();

        // Act
        var service = new ScanItemsService(_projectRules, _repositoryRules, _buildPipelineRules,
            _yamlReleasePipelineRules, _classicReleasePipelineRules, _config, _repositoryService.Object);
        var result = await service.ScanYamlReleasePipelinesAsync(organization, project, yamlPipelines, ciIdentifier);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ScanYamlPipelines_ShouldReturn_CorrectResult(bool scanResult)
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var yamlPipeline = _fixture.Create<BuildDefinition>();
        yamlPipeline.Project.Id = project.Id;
        var yamlPipelines = new[] { yamlPipeline };
        var ciIdentifier = _fixture.Create<string>();

        _yamlReleasePipelineRule
            .Setup(x => x.EvaluateAsync(organization, project.Id, yamlPipeline))
            .ReturnsAsync(scanResult)
            .Verifiable();

        _yamlReleasePipelineRule
            .Setup(x => x.Name)
            .Returns(RuleNames.YamlReleasePipelineHasRequiredRetentionPolicy);

        // Act
        var service = new ScanItemsService(_projectRules, _repositoryRules, _buildPipelineRules,
            _yamlReleasePipelineRules, _classicReleasePipelineRules, _config, _repositoryService.Object);
        var result = await service.ScanYamlReleasePipelinesAsync(organization, project, yamlPipelines, ciIdentifier);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(_yamlReleasePipelineRules.Count());
        var evaluatedRule = result.First();
        evaluatedRule.Status.ShouldBe(scanResult);
        evaluatedRule.Item.Id.ShouldBe(yamlPipeline.Id);
        evaluatedRule.Item.Name.ShouldBe(yamlPipeline.Name);
        evaluatedRule.Item.ProjectId.ShouldBe(project.Id);
        _yamlReleasePipelineRule.Verify();
    }

    [Fact]
    public async Task NoClassicPipelines_ShouldReturn_NoRules()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var classicPipelines = Enumerable.Empty<ReleaseDefinition>();
        var ciIdentifier = _fixture.Create<string>();

        // Act
        var service = new ScanItemsService(_projectRules, _repositoryRules, _buildPipelineRules,
            _yamlReleasePipelineRules, _classicReleasePipelineRules, _config, _repositoryService.Object);
        var result = await service.ScanClassicReleasePipelinesAsync(organization, project, classicPipelines, ciIdentifier);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ScanClassicPipelines_ShouldReturn_CorrectResult(bool scanResult)
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var classicPipeline = _fixture.Create<ReleaseDefinition>();
        var classicPipelines = new[] { classicPipeline };
        var ciIdentifier = _fixture.Create<string>();

        _classicReleasePipelineRule
            .Setup(x => x.EvaluateAsync(organization, project.Id, classicPipeline))
            .ReturnsAsync(scanResult)
            .Verifiable();

        _classicReleasePipelineRule
            .Setup(x => x.Name)
            .Returns(RuleNames.ClassicReleasePipelineHasRequiredRetentionPolicy);

        // Act
        var service = new ScanItemsService(_projectRules, _repositoryRules, _buildPipelineRules,
            _yamlReleasePipelineRules, _classicReleasePipelineRules, _config, _repositoryService.Object);
        var result = await service.ScanClassicReleasePipelinesAsync(organization, project, classicPipelines, ciIdentifier);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(_classicReleasePipelineRules.Count());
        var evaluatedRule = result.First();
        evaluatedRule.Status.ShouldBe(scanResult);
        evaluatedRule.Item.Id.ShouldBe(classicPipeline.Id);
        evaluatedRule.Item.Name.ShouldBe(classicPipeline.Name);
        evaluatedRule.Item.ProjectId.ShouldBe(project.Id);
        _classicReleasePipelineRule.Verify();
    }
}
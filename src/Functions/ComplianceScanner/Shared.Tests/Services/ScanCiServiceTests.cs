using AutoFixture;
using Moq;
using Rabobank.Compliancy.Core.PipelineResources.Services;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Helpers;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Xunit;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Tests.Services;

public class ScanCiServiceTests
{
    private readonly IFixture _fixture = new Fixture() { RepeatCount = 1 };
    private readonly Mock<IAzdoRestClient> _azdoClient = new();
    private readonly Mock<IPipelinesService> _pipelineService = new();
    private readonly Mock<IScanItemsService> _scanItemsService = new();
    private readonly Mock<IVerifyComplianceService> _verifyComplianceService = new();
    private readonly ComplianceConfig _config;
    private readonly ScanCiService _sut;
    private readonly Mock<IReleasePipelineService> _releasePipelineService = new();
    private readonly Mock<IBuildPipelineService> _buildPipelineService = new();

    public ScanCiServiceTests()
    {
        _config = _fixture.Create<ComplianceConfig>();
        _sut = new ScanCiService(_azdoClient.Object, _pipelineService.Object, _scanItemsService.Object,
            _verifyComplianceService.Object, _config, _releasePipelineService.Object, _buildPipelineService.Object);
    }

    [Fact]
    public async Task IfNoCorrectRegisteredPipelines_ShouldScanNoItems()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var ciIdentifier = _fixture.Create<string>();
        var scanDate = _fixture.Create<DateTime>();
        var allClassicReleasePipelines = _fixture.CreateMany<ReleaseDefinition>().ToList();
        var allYamlReleasePipelines = _fixture.CreateMany<BuildDefinition>().ToList();
        var registration = _fixture.Create<PipelineRegistration>();
        registration.CiIdentifier = ciIdentifier;
        var registrations = new List<PipelineRegistration> { registration };

        _azdoClient
            .Setup(a => a.GetAsync(It.Is<IAzdoRequest<Project>>(request => request.Resource.Contains(project.Id)), organization))
            .ReturnsAsync(project);
        _pipelineService
            .Setup(p => p.GetAllYamlPipelinesAsync(organization, project.Id, It.IsAny<IEnumerable<PipelineRegistration>>()))
            .ReturnsAsync(allYamlReleasePipelines)
            .Verifiable();
        _pipelineService
            .Setup(p => p.GetClassicReleasePipelinesAsync(organization, project.Id, It.IsAny<IEnumerable<PipelineRegistration>>()))
            .ReturnsAsync(allClassicReleasePipelines)
            .Verifiable();

        var service = new ScanCiService(_azdoClient.Object, _pipelineService.Object, _scanItemsService.Object,
            _verifyComplianceService.Object, _config, _releasePipelineService.Object, _buildPipelineService.Object);

        // Act
        var result = await service.ScanCiAsync(organization, project, ciIdentifier, scanDate, registrations);

        // Assert
        _pipelineService.Verify();

        result.IsScanFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task IfCorrectRegistered_ClassicPipeline_ShouldScanItems_AndCreateReport()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var ciIdentifier = _fixture.Create<string>();
        var scanDate = _fixture.Create<DateTime>();
        var classicPipelineId = _fixture.Create<string>();
        var classicPipelineStageId = _fixture.Create<int>();
        var isSox = _fixture.Create<bool>();
        var itemScanResults = _fixture.CreateMany<EvaluatedRule>().ToList();
        var principleReports = _fixture.CreateMany<PrincipleReport>().ToList();

        _fixture.Customize<PipelineRegistration>(d => d
            .With(x => x.CiIdentifier, ciIdentifier)
            .With(x => x.StageId, classicPipelineStageId.ToString())
            .With(x => x.PipelineId, classicPipelineId)
            .With(x => x.IsSoxApplication, isSox)
            .With(x => x.PipelineType, "Classic release"));
        _fixture.Customize<ReleaseDefinition>(r => r
            .With(r => r.Id, classicPipelineId));
        _fixture.Customize<ReleaseDefinitionEnvironment>(e => e
            .With(e => e.Id, classicPipelineStageId));

        var repository = _fixture.Build<Repository>()
            .With(r => r.Type, RepositoryTypes.TfsGit)
            .With(r => r.Url, new Uri($"https://dev.azure.com/{organization}/{project.Name}"))
            .Create();
        var buildPipeline = _fixture.Build<BuildDefinition>()
            .With(b => b.Repository, repository)
            .Create();

        var allClassicReleasePipelines = _fixture.CreateMany<ReleaseDefinition>();
        var allYamlReleasePipelines = _fixture.CreateMany<BuildDefinition>();
        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();

        _azdoClient
            .Setup(a => a.GetAsync(It.Is<IAzdoRequest<Project>>(request => request.Resource.Contains(project.Id)), organization))
            .ReturnsAsync(project);
        foreach (var pipeline in allClassicReleasePipelines)
        {
            _azdoClient
                .Setup(a => a.GetAsync(It.Is<IAzdoRequest<ReleaseDefinition>>(request => request.Resource.Contains(project.Id) && request.Resource.Contains(pipeline.Id)), organization))
                .ReturnsAsync(pipeline)
                .Verifiable();
        }
        _pipelineService
            .Setup(p => p.GetAllYamlPipelinesAsync(organization, project.Id, It.IsAny<IEnumerable<PipelineRegistration>>()))
            .ReturnsAsync(allYamlReleasePipelines)
            .Verifiable();
        _pipelineService
            .Setup(p => p.GetClassicReleasePipelinesAsync(organization, project.Id, It.IsAny<IEnumerable<PipelineRegistration>>()))
            .ReturnsAsync(allClassicReleasePipelines)
            .Verifiable();
        _releasePipelineService
            .Setup(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<ReleaseDefinition>(), It.IsAny<string>(), It.IsAny<IEnumerable<BuildDefinition>>()))
            .ReturnsAsync(new[] { buildPipeline })
            .Verifiable();
        _releasePipelineService
            .Setup(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ReleaseDefinition>>(), It.IsAny<IEnumerable<BuildDefinition>>()))
            .ReturnsAsync(new[] { repository })
            .Verifiable();
        _scanItemsService
            .Setup(x => x.ScanProjectAsync(organization, project, ciIdentifier))
            .ReturnsAsync(itemScanResults)
            .Verifiable();
        _scanItemsService
            .Setup(x => x.ScanRepositoriesAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<IEnumerable<Repository>>(), ciIdentifier))
            .ReturnsAsync(itemScanResults);
        _scanItemsService
            .Setup(x => x.ScanBuildPipelinesAsync(organization, project, new[] { buildPipeline }, ciIdentifier, null))
            .ReturnsAsync(itemScanResults)
            .Verifiable();
        _scanItemsService
            .Setup(x => x.ScanYamlReleasePipelinesAsync(organization, project, new BuildDefinition[] { }, ciIdentifier))
            .ReturnsAsync(itemScanResults)
            .Verifiable();
        _scanItemsService
            .Setup(x => x.ScanClassicReleasePipelinesAsync(organization, project, allClassicReleasePipelines, ciIdentifier))
            .ReturnsAsync(itemScanResults)
            .Verifiable();
        _verifyComplianceService
            .Setup(x => x.CreatePrincipleReports(It.IsAny<IEnumerable<EvaluatedRule>>(), scanDate))
            .Returns(principleReports)
            .Verifiable();

        var service = new ScanCiService(_azdoClient.Object, _pipelineService.Object, _scanItemsService.Object,
            _verifyComplianceService.Object, _config, _releasePipelineService.Object, _buildPipelineService.Object);

        // Act
        var result = await service.ScanCiAsync(organization, project, ciIdentifier, scanDate, registrations);

        // Assert
        _releasePipelineService.Verify();
        _azdoClient.Verify();
        _verifyComplianceService.Verify();
        _scanItemsService.Verify(x => x.ScanRepositoriesAsync(organization, project
            , It.Is<IEnumerable<Repository>>(r => r.Contains(repository) &&
                                                  r.Any(repo => repo.Type == RepositoryTypes.TfsGit) && r.Count() == 1), ciIdentifier));

        result.ShouldNotBeNull();
        result.ScanDate.ShouldBe(scanDate);
        result.Id.ShouldBe(ciIdentifier);
        result.IsSOx.ShouldBe(isSox);
        result.PrincipleReports.ShouldBe(principleReports);
    }

    [Fact]
    public async Task IfCorrectRegistered_YamlPipeline_ShouldScanItems_AndCreateReport()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var ciIdentifier = _fixture.Create<string>();
        var scanDate = _fixture.Create<DateTime>();
        var yamlPipelineId = _fixture.Create<string>();
        var yamlPipelineStageId = _fixture.Create<int>();
        var isSox = _fixture.Create<bool>();
        var itemScanResults = _fixture.CreateMany<EvaluatedRule>().ToList();
        var principleReports = _fixture.CreateMany<PrincipleReport>().ToList();

        _fixture.Customize<PipelineRegistration>(d => d
            .With(x => x.CiIdentifier, ciIdentifier)
            .With(x => x.StageId, yamlPipelineStageId.ToString())
            .With(x => x.PipelineId, yamlPipelineId)
            .With(x => x.IsSoxApplication, isSox)
            .With(x => x.PipelineType, "Classic release"));
        _fixture.Customize<BuildDefinition>(y => y
            .With(y => y.Id, yamlPipelineId)
            .With(b => b.PipelineType, "YAML pipeline with stages"));
        _fixture.Customize<Stage>(s => s
            .With(s => s.Id, yamlPipelineStageId.ToString()));
        _fixture.Customize<ReleaseDefinitionEnvironment>(rde => rde
            .With(rde => rde.Id, yamlPipelineStageId));

        var repository = _fixture.Build<Repository>()
            .With(r => r.Type, RepositoryTypes.TfsGit)
            .With(r => r.Id, "1")
            .With(r => r.Url, new Uri($"https://dev.azure.com/{organization}/{project.Name}"))
            .Create();
        var buildPipeline = _fixture.Build<BuildDefinition>()
            .With(r => r.Id, yamlPipelineId)
            .With(b => b.Repository, repository)
            .Create();

        var repository2 = _fixture.Build<Repository>()
            .With(r => r.Type, RepositoryTypes.TfsGit)
            .With(r => r.Id, "2")
            .With(r => r.Url, new Uri($"https://dev.azure.com/{organization}/{project.Name}"))
            .Create();

        var allClassicReleasePipelines = _fixture.CreateMany<ReleaseDefinition>().ToList();
        var allYamlReleasePipelines = _fixture.CreateMany<BuildDefinition>().ToList();
        allYamlReleasePipelines.First().Repository = repository2;
        var allBuildPipelines = _fixture.CreateMany<BuildDefinition>().ToList();
        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();

        _azdoClient
            .Setup(a => a.GetAsync(It.Is<IAzdoRequest<Project>>(request => request.Resource.Contains(project.Id)), organization))
            .ReturnsAsync(project);
        foreach (var pipeline in allClassicReleasePipelines)
        {
            _azdoClient
                .Setup(a => a.GetAsync(It.Is<IAzdoRequest<ReleaseDefinition>>(request => request.Resource.Contains(project.Id) && request.Resource.Contains(pipeline.Id)), organization))
                .ReturnsAsync(pipeline)
                .Verifiable();
        }
        _pipelineService
            .Setup(p => p.GetAllYamlPipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(allYamlReleasePipelines)
            .Verifiable();
        _pipelineService
            .Setup(p => p.GetClassicReleasePipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(allClassicReleasePipelines)
            .Verifiable();
        _buildPipelineService
            .Setup(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<BuildDefinition>(), allBuildPipelines))
            .ReturnsAsync(new[] { buildPipeline })
            .Verifiable();
        _releasePipelineService
            .Setup(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ReleaseDefinition>>(), It.IsAny<IEnumerable<BuildDefinition>>()))
            .ReturnsAsync(new[] { repository, repository2 })
            .Verifiable();
        _scanItemsService
            .Setup(x => x.ScanProjectAsync(organization, project, ciIdentifier))
            .ReturnsAsync(itemScanResults)
            .Verifiable();
        _scanItemsService
            .Setup(x => x.ScanRepositoriesAsync(organization, project, It.IsAny<IEnumerable<Repository>>(), ciIdentifier))
            .ReturnsAsync(itemScanResults);
        _scanItemsService
            .Setup(x => x.ScanBuildPipelinesAsync(organization, project, new[] { buildPipeline }, ciIdentifier,
                It.IsAny<IEnumerable<RuleProfile>>()))
            .ReturnsAsync(itemScanResults)
            .Verifiable();
        _scanItemsService
            .Setup(x => x.ScanYamlReleasePipelinesAsync(organization, project, allYamlReleasePipelines, ciIdentifier))
            .ReturnsAsync(itemScanResults)
            .Verifiable();
        _scanItemsService
            .Setup(x => x.ScanClassicReleasePipelinesAsync(organization, project, It.IsAny<IEnumerable<ReleaseDefinition>>(), ciIdentifier))
            .ReturnsAsync(itemScanResults)
            .Verifiable();
        _verifyComplianceService
            .Setup(x => x.CreatePrincipleReports(It.IsAny<IEnumerable<EvaluatedRule>>(), scanDate))
            .Returns(principleReports)
            .Verifiable();

        var service = new ScanCiService(_azdoClient.Object, _pipelineService.Object, _scanItemsService.Object,
            _verifyComplianceService.Object, _config, _releasePipelineService.Object, _buildPipelineService.Object);

        // Act
        var result = await service.ScanCiAsync(organization, project, ciIdentifier, scanDate, registrations);

        // Assert
        _pipelineService.Verify();
        _releasePipelineService.Verify();
        _buildPipelineService.Verify();
        _azdoClient.Verify();
        _scanItemsService.Verify();
        _verifyComplianceService.Verify();
        _scanItemsService.Verify(v => v.ScanRepositoriesAsync(organization, project,
            It.Is<IEnumerable<Repository>>(r => r.Contains(repository) && r.Contains(repository2)), ciIdentifier));

        result.ShouldNotBeNull();
        result.ScanDate.ShouldBe(scanDate);
        result.Id.ShouldBe(ciIdentifier);
        result.IsSOx.ShouldBe(isSox);
        result.PrincipleReports.ShouldBe(principleReports);
    }

    [Fact]
    public async Task ScanNonProdPipelineAsync_OneNonProdPipelineToBeScanned_CreatesReport()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var scanDate = _fixture.Create<DateTime>();
        var yamlPipelineId = _fixture.Create<string>();
        var yamlPipelineStageId = _fixture.Create<string>();
        var itemScanResults = _fixture.CreateMany<EvaluatedRule>().ToList();
        var principleReports = _fixture.CreateMany<PrincipleReport>().ToList();
        var nonProdPipelineRegistration = _fixture.Create<PipelineRegistration>();
        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();
        nonProdPipelineRegistration.PipelineId = yamlPipelineId;
        const string yamlPipelineName = "pipeline1";

        _fixture.Customize<PipelineRegistration>(d => d
            .Without(x => x.CiIdentifier)
            .With(x => x.ToBeScanned, true)
            .With(x => x.StageId, yamlPipelineStageId)
            .With(x => x.PipelineId, yamlPipelineId));

        _fixture.Customize<BuildDefinition>(y => y
            .With(y => y.Id, yamlPipelineId)
            .With(b => b.Name, yamlPipelineName)
            .With(b => b.PipelineType, "YAML pipeline with stages"));

        var repository = _fixture.Create<Repository>();
        var buildPipeline = _fixture.Build<BuildDefinition>()
            .With(b => b.Repository, repository)
            .Create();

        var allYamlReleasePipelines = _fixture.CreateMany<BuildDefinition>().ToList();
        var allBuildPipelines = _fixture.CreateMany<BuildDefinition>().ToList();

        _pipelineService
            .Setup(p => p.GetAllYamlPipelinesAsync(organization, project.Id, It.IsAny<IEnumerable<PipelineRegistration>>()))
            .ReturnsAsync(allYamlReleasePipelines)
            .Verifiable();
        _buildPipelineService
            .Setup(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<BuildDefinition>(), allBuildPipelines))
            .ReturnsAsync(new[] { buildPipeline })
            .Verifiable();
        _releasePipelineService
            .Setup(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ReleaseDefinition>>(), It.IsAny<IEnumerable<BuildDefinition>>()))
            .ReturnsAsync(new[] { repository })
            .Verifiable();
        _scanItemsService
            .Setup(x => x.ScanProjectAsync(organization, project, null))
            .ReturnsAsync(itemScanResults)
            .Verifiable();
        _scanItemsService
            .Setup(x => x.ScanRepositoriesAsync(organization, project, It.IsAny<IEnumerable<Repository>>(), null))
            .ReturnsAsync(itemScanResults)
            .Verifiable();
        _scanItemsService
            .Setup(x => x.ScanBuildPipelinesAsync(organization, project, new[] { buildPipeline }, null,
                It.IsAny<IEnumerable<RuleProfile>>()))
            .ReturnsAsync(itemScanResults)
            .Verifiable();
        _scanItemsService
            .Setup(x => x.ScanYamlReleasePipelinesAsync(organization, project, allYamlReleasePipelines, null))
            .ReturnsAsync(itemScanResults)
            .Verifiable();
        _scanItemsService
            .Setup(x => x.ScanClassicReleasePipelinesAsync(organization, project, new ReleaseDefinition[] { }, null))
            .ReturnsAsync(itemScanResults)
            .Verifiable();
        _verifyComplianceService
            .Setup(x => x.CreatePrincipleReports(It.IsAny<IEnumerable<EvaluatedRule>>(), scanDate))
            .Returns(principleReports)
            .Verifiable();
        _azdoClient
            .Setup(a => a.GetAsync(It.Is<IAzdoRequest<Project>>(request => request.Resource.Contains(project.Id)), organization))
            .ReturnsAsync(project);

        var service = new ScanCiService(_azdoClient.Object, _pipelineService.Object, _scanItemsService.Object,
            _verifyComplianceService.Object, _config, _releasePipelineService.Object, _buildPipelineService.Object);

        // Act
        var result = await service.ScanNonProdPipelineAsync(organization, project, scanDate, nonProdPipelineRegistration.PipelineId, registrations);

        // Assert
        _pipelineService.Verify();
        _azdoClient.Verify();
        _releasePipelineService.Verify();
        _buildPipelineService.Verify();
        _scanItemsService.Verify();
        _verifyComplianceService.Verify();

        result.ShouldNotBeNull();
        result.Date.ShouldBe(scanDate);
        result.PipelineId.ShouldBe(yamlPipelineId);
        result.PipelineType.ShouldBe(ItemTypes.YamlReleasePipeline);
        result.PipelineName.ShouldBe(yamlPipelineName);
        result.PrincipleReports.ShouldBe(principleReports);
    }

    [Fact]
    public async Task ScanCiAsync_WithNoPipelineRegistrations_ShouldReturnEmptyCiReport()
    {
        // Arrange
        var ciIdentifier = _fixture.Create<string>();
        var exception = new Exception($"There are no pipeline registrations for CI: {ciIdentifier}");
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var scanDate = _fixture.Create<DateTime>();
        var registrations = Enumerable.Empty<PipelineRegistration>();

        // Act
        var actual = await _sut.ScanCiAsync(organization, project, ciIdentifier, scanDate, registrations);

        // Assert
        actual.ShouldBeEquivalentTo(new CiReport(ciIdentifier, null, scanDate)
        {
            IsScanFailed = true,
            RescanUrl = CreateUrl.CiRescanUrl(_config, organization, project.Id, ciIdentifier),
            ScanException = new ExceptionSummaryReport(exception),
            PrincipleReports = Enumerable.Empty<PrincipleReport>()
        });
    }

    [Fact]
    public async Task ScanCIAsync_PipelineRegistrationWithDifferentCasingStageId_CreateReport()
    {
        // Arrange
        var ciIdentifier = _fixture.Create<string>();
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var scanDate = _fixture.Create<DateTime>();
        var itemScanResults = _fixture.CreateMany<EvaluatedRule>().ToList();

        _fixture.Customize<PipelineRegistration>(d => d
            .With(x => x.CiIdentifier, ciIdentifier)
            .With(x => x.StageId, "Production")
            .With(x => x.PipelineId, "1")
            .With(x => x.RuleProfileName, "Default"));

        _fixture.Customize<BuildDefinition>(x => x
            .With(x => x.Stages, new List<Stage> { new() { Id = "production" } })
            .With(x => x.Id, "1")
            .With(x => x.PipelineType, ItemTypes.YamlPipelineWithStages));

        var allYamlReleasePipelines = _fixture.CreateMany<BuildDefinition>().ToList();
        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();

        _pipelineService
            .Setup(p => p.GetAllYamlPipelinesAsync(organization, project.Id, It.IsAny<IEnumerable<PipelineRegistration>>()))
            .ReturnsAsync(allYamlReleasePipelines)
            .Verifiable();

        _scanItemsService
            .Setup(m => m.ScanBuildPipelinesAsync(organization, project, It.IsAny<IEnumerable<BuildDefinition>>(),
                ciIdentifier, It.IsAny<IEnumerable<RuleProfile>>()))
            .ReturnsAsync(itemScanResults);

        _scanItemsService
            .Setup(x => x.ScanProjectAsync(organization, project, ciIdentifier))
            .ReturnsAsync(itemScanResults);

        // Act
        var ciReport = await _sut.ScanCiAsync(organization, project, ciIdentifier, scanDate, registrations);

        // Assert
        ciReport.ScanException.ShouldBeNull();
    }
}
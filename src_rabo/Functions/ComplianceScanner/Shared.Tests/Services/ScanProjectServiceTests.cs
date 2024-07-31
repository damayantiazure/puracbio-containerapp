using AutoFixture;
using Moq;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Tests.Services;

public class ScanProjectServiceTests
{
    private readonly Mock<IAzdoRestClient> _azdoClientMock = new();
    private readonly Mock<ICompliancyReportService> _compliancyReportServiceMock = new();
    private readonly ComplianceConfig _config;
    private readonly IFixture _fixture = new Fixture { RepeatCount = 1 };
    private readonly Mock<IPipelinesService> _pipelinesServiceMock = new();
    private readonly Mock<IPipelineRegistrationRepository> _registrationRepoMock = new();
    private readonly Mock<IScanCiService> _scanCiServiceMock = new();

    public ScanProjectServiceTests() => _config = _fixture.Create<ComplianceConfig>();

    [Fact]
    public async Task ShouldMark_UnregisteredYamlPipeline_AsUnregistered_AndNotExecuteScan()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Build<Project>()
            .With(f => f.Id, _fixture.Create<Guid>().ToString)
            .Create();
        var scanDate = _fixture.Create<DateTime>();
        var parallelCiScans = _fixture.Create<int>();
        var yamlPipelineId = _fixture.Create<string>();
        var yamlPipelineStageId = _fixture.Create<string>();

        _fixture.Customize<BuildDefinition>(ctx => ctx
            .With(p => p.Id, yamlPipelineId)
            .With(x => x.PipelineType, ItemTypes.YamlPipelineWithStages));
        _fixture.Customize<Stage>(ctx => ctx
            .With(s => s.Id, yamlPipelineStageId));

        _fixture.Customize<PipelineRegistration>(ctx => ctx
            .With(d => d.ToBeScanned, (bool?)null));

        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();
        var classicReleasePipelines = _fixture.CreateMany<ReleaseDefinition>(0).ToList();
        var yamlReleasePipelines = _fixture.CreateMany<BuildDefinition>().ToList();
        yamlReleasePipelines.First().PipelineRegistrations = new List<PipelineRegistration>();
        var classicBuildPipelines = _fixture.CreateMany<BuildDefinition>().ToList();

        _registrationRepoMock
            .Setup(x => x.GetAsync(organization, project.Id))
            .ReturnsAsync(registrations)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetClassicReleasePipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(classicReleasePipelines)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetAllYamlPipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(yamlReleasePipelines)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetClassicBuildPipelinesAsync(organization, project.Id))
            .ReturnsAsync(classicBuildPipelines)
            .Verifiable();

        var sut = new ScanProjectService(_azdoClientMock.Object, _registrationRepoMock.Object,
            _pipelinesServiceMock.Object, _scanCiServiceMock.Object,
            _compliancyReportServiceMock.Object, _config);

        // Act
        await sut.ScanProjectAsync(organization, project, scanDate, parallelCiScans);

        // Assert
        _registrationRepoMock.Verify();
        _azdoClientMock.Verify();
        _pipelinesServiceMock.Verify();
        _scanCiServiceMock.Verify();

        _compliancyReportServiceMock
            .Verify(x => x.UpdateComplianceReportAsync(organization, Guid.Parse(project.Id), It.Is<CompliancyReport>(
                    compliancyReport =>
                        compliancyReport.UnregisteredPipelines.Count == 1 &&
                        compliancyReport.UnregisteredPipelines.First().Id == yamlPipelineId &&
                        compliancyReport.UnregisteredPipelines.First().Type == "YAML release" &&
                        compliancyReport.UnregisteredPipelines.First().Stages
                            .Select(stageReport => stageReport.Id)
                            .Contains(yamlPipelineStageId) &&
                        compliancyReport.RegisteredConfigurationItems.Count == 0 &&
                        compliancyReport.RegisteredPipelines.Count == 0),
                scanDate), Times.Once);

        _scanCiServiceMock
            .Verify(x => x.ScanCiAsync(organization, project, It.IsAny<string>(), scanDate, registrations),
                Times.Never);
    }

    [Fact]
    public async Task ShouldMark_UnregisteredClassicPipeline_AsUnregistered_AndNotExecuteScan()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Build<Project>()
            .With(f => f.Id, _fixture.Create<Guid>().ToString)
            .Create();
        var scanDate = _fixture.Create<DateTime>();
        var parallelCiScans = _fixture.Create<int>();
        var classicPipelineId = _fixture.Create<string>();
        var classicPipelineStageId = _fixture.Create<int>();

        _fixture.Customize<ReleaseDefinition>(ctx => ctx
            .With(r => r.Id, classicPipelineId));
        _fixture.Customize<ReleaseDefinitionEnvironment>(ctx => ctx
            .With(e => e.Id, classicPipelineStageId));

        _fixture.Customize<PipelineRegistration>(ctx => ctx
            .With(d => d.ToBeScanned, (bool?)null));

        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();
        var classicReleasePipelines = _fixture.CreateMany<ReleaseDefinition>().ToList();
        classicReleasePipelines.First().PipelineRegistrations = new List<PipelineRegistration>();
        var yamlReleasePipelines = _fixture.CreateMany<BuildDefinition>(0).ToList();
        var classicBuildPipelines = _fixture.CreateMany<BuildDefinition>().ToList();

        _registrationRepoMock
            .Setup(x => x.GetAsync(organization, project.Id))
            .ReturnsAsync(registrations)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetClassicReleasePipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(classicReleasePipelines)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetAllYamlPipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(yamlReleasePipelines)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetClassicBuildPipelinesAsync(organization, project.Id))
            .ReturnsAsync(classicBuildPipelines)
            .Verifiable();

        var sut = new ScanProjectService(_azdoClientMock.Object, _registrationRepoMock.Object,
            _pipelinesServiceMock.Object, _scanCiServiceMock.Object,
            _compliancyReportServiceMock.Object, _config);

        // Act
        await sut.ScanProjectAsync(organization, project, scanDate, parallelCiScans);

        // Assert
        _registrationRepoMock.Verify();
        _azdoClientMock.Verify();
        _pipelinesServiceMock.Verify();
        _scanCiServiceMock.Verify();

        _compliancyReportServiceMock
            .Verify(compliancyReportService => compliancyReportService.UpdateComplianceReportAsync(organization,
                Guid.Parse(project.Id), It.Is<CompliancyReport>(
                    compliancyReport =>
                        compliancyReport.UnregisteredPipelines.Count == 1 &&
                        compliancyReport.UnregisteredPipelines.First().Id == classicPipelineId &&
                        compliancyReport.UnregisteredPipelines.First().Type == "Classic release" &&
                        compliancyReport.UnregisteredPipelines.First().Stages
                            .Select(stageReport => stageReport.Id)
                            .Contains(classicPipelineStageId.ToString()) &&
                        compliancyReport.RegisteredConfigurationItems.Count == 0 &&
                        compliancyReport.RegisteredPipelines.Count == 0),
                scanDate), Times.Once());

        _scanCiServiceMock
            .Verify(x => x.ScanCiAsync(organization, project, It.IsAny<string>(), scanDate, registrations),
                Times.Never);
    }

    [Fact]
    public async Task ShouldMark_RegisteredYamlPipeline_WithMissingProdStage_AsInvalidRegistration_AndNotExecuteScan()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Build<Project>()
            .With(f => f.Id, _fixture.Create<Guid>().ToString)
            .Create();
        var scanDate = _fixture.Create<DateTime>();
        var parallelCiScans = _fixture.Create<int>();
        var yamlPipelineId = _fixture.Create<string>();
        var yamlPipelineStageId = _fixture.Create<string>();
        var registeredProdStage = _fixture.Create<string>();
        var ciIdentifier = _fixture.Create<string>();
        const string pipelineType = ItemTypes.YamlReleasePipeline;

        _fixture.Customize<BuildDefinition>(ctx => ctx
            .With(p => p.Id, yamlPipelineId)
            .With(x => x.PipelineType, ItemTypes.YamlPipelineWithStages));
        _fixture.Customize<Stage>(ctx => ctx
            .With(s => s.Id, yamlPipelineStageId));

        _fixture.Customize<PipelineRegistration>(ctx => ctx
            .With(d => d.PipelineId, yamlPipelineId)
            .With(d => d.StageId, registeredProdStage)
            .With(d => d.CiIdentifier, ciIdentifier)
            .With(d => d.PipelineType, pipelineType)
            .With(d => d.ToBeScanned, (bool?)null)
            .With(d => d.PartitionKey, PipelineRegistration.Prod));

        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();
        var classicReleasePipelines = _fixture.CreateMany<ReleaseDefinition>(0).ToList();
        var yamlReleasePipelines = _fixture.CreateMany<BuildDefinition>().ToList();
        var classicBuildPipelines = _fixture.CreateMany<BuildDefinition>().ToList();

        _registrationRepoMock
            .Setup(x => x.GetAsync(organization, project.Id))
            .ReturnsAsync(registrations)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetClassicReleasePipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(classicReleasePipelines)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetAllYamlPipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(yamlReleasePipelines)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetClassicBuildPipelinesAsync(organization, project.Id))
            .ReturnsAsync(classicBuildPipelines)
            .Verifiable();

        var sut = new ScanProjectService(_azdoClientMock.Object, _registrationRepoMock.Object,
            _pipelinesServiceMock.Object, _scanCiServiceMock.Object,
            _compliancyReportServiceMock.Object, _config);

        // Act
        await sut.ScanProjectAsync(organization, project, scanDate, parallelCiScans);

        // Assert
        _registrationRepoMock.Verify();
        _azdoClientMock.Verify();
        _pipelinesServiceMock.Verify();
        _scanCiServiceMock.Verify();
        _compliancyReportServiceMock
            .Verify(compliancyReportService => compliancyReportService.UpdateComplianceReportAsync(organization,
                Guid.Parse(project.Id), It.Is<CompliancyReport>(x =>
                    x.UnregisteredPipelines.Count == 0 &&
                    x.RegisteredPipelinesNoProdStage.Count == 1 &&
                    x.RegisteredPipelinesNoProdStage.First().Id == yamlPipelineId &&
                    x.RegisteredPipelinesNoProdStage.First().Type == "YAML release" &&
                    x.RegisteredPipelinesNoProdStage.First().Stages
                        .Select(stageReport => stageReport.Id)
                        .Contains(yamlPipelineStageId) &&
                    x.RegisteredConfigurationItems.Count == 0 &&
                    x.RegisteredPipelinesNoProdStage.First().CiIdentifiers == ciIdentifier &&
                    x.RegisteredPipelines.Count == 0),
                scanDate), Times.Once);
        _scanCiServiceMock
            .Verify(x => x.ScanCiAsync(organization, project, It.IsAny<string>(), scanDate, registrations),
                Times.Never);
    }

    [Fact]
    public async Task
        ShouldMark_RegisteredClassicPipeline_WithMissingProdStage_AsInvalidRegistration_AndNotExecuteScan()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Build<Project>()
            .With(f => f.Id, _fixture.Create<Guid>().ToString)
            .Create();
        var scanDate = _fixture.Create<DateTime>();
        var parallelCiScans = _fixture.Create<int>();
        var classicPipelineId = _fixture.Create<string>();
        var classicPipelineStageId = _fixture.Create<int>();
        var registeredProdStage = _fixture.Create<string>();
        var ciIdentifier = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;

        _fixture.Customize<ReleaseDefinition>(ctx => ctx
            .With(r => r.Id, classicPipelineId));
        _fixture.Customize<ReleaseDefinitionEnvironment>(ctx => ctx
            .With(e => e.Id, classicPipelineStageId));

        _fixture.Customize<PipelineRegistration>(ctx => ctx
            .With(d => d.PipelineId, classicPipelineId)
            .With(d => d.StageId, registeredProdStage)
            .With(d => d.CiIdentifier, ciIdentifier)
            .With(d => d.PipelineType, pipelineType)
            .With(d => d.ToBeScanned, (bool?)null)
            .With(d => d.PartitionKey, PipelineRegistration.Prod));

        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();
        var classicReleasePipelines = _fixture.CreateMany<ReleaseDefinition>().ToList();
        var yamlReleasePipelines = _fixture.CreateMany<BuildDefinition>(0).ToList();
        var classicBuildPipelines = _fixture.CreateMany<BuildDefinition>().ToList();

        _registrationRepoMock
            .Setup(x => x.GetAsync(organization, project.Id))
            .ReturnsAsync(registrations)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetClassicReleasePipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(classicReleasePipelines)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetAllYamlPipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(yamlReleasePipelines)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetClassicBuildPipelinesAsync(organization, project.Id))
            .ReturnsAsync(classicBuildPipelines)
            .Verifiable();

        var sut = new ScanProjectService(_azdoClientMock.Object, _registrationRepoMock.Object,
            _pipelinesServiceMock.Object, _scanCiServiceMock.Object,
            _compliancyReportServiceMock.Object, _config);

        // Act
        await sut.ScanProjectAsync(organization, project, scanDate, parallelCiScans);

        // Assert
        _registrationRepoMock.Verify();
        _azdoClientMock.Verify();
        _pipelinesServiceMock.Verify();
        _scanCiServiceMock.Verify();
        _compliancyReportServiceMock
            .Verify(compliancyReportService => compliancyReportService.UpdateComplianceReportAsync(organization,
                Guid.Parse(project.Id), It.Is<CompliancyReport>(x =>
                    x.UnregisteredPipelines.Count == 0 &&
                    x.RegisteredPipelinesNoProdStage.Count == 1 &&
                    x.RegisteredPipelinesNoProdStage.First().Id == classicPipelineId &&
                    x.RegisteredPipelinesNoProdStage.First().Type == "Classic release" &&
                    x.RegisteredPipelinesNoProdStage.First().Stages
                        .Select(stageReport => stageReport.Id)
                        .Contains(classicPipelineStageId.ToString()) &&
                    x.RegisteredConfigurationItems.Count == 0 &&
                    x.RegisteredPipelines.Count == 0 &&
                    x.RegisteredPipelinesNoProdStage.First().CiIdentifiers == ciIdentifier),
                scanDate), Times.Once);
        _scanCiServiceMock
            .Verify(x => x.ScanCiAsync(organization, project, It.IsAny<string>(), scanDate, registrations),
                Times.Never);
    }

    [Fact]
    public async Task ShouldMark_RegisteredYamlPipeline_AsRegistered_AndExecuteScan()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Build<Project>()
            .With(f => f.Id, _fixture.Create<Guid>().ToString)
            .Create();
        var scanDate = _fixture.Create<DateTime>();
        var parallelCiScans = _fixture.Create<int>();
        var yamlPipelineId = _fixture.Create<string>();
        var yamlPipelineStageId = _fixture.Create<string>();
        var ciIdentifier = _fixture.Create<string>();
        const string pipelineType = ItemTypes.YamlReleasePipeline;
        var ciReport = _fixture.Create<CiReport>();

        _fixture.Customize<BuildDefinition>(ctx => ctx
            .With(p => p.Id, yamlPipelineId)
            .With(x => x.PipelineType, ItemTypes.YamlPipelineWithStages));
        _fixture.Customize<Stage>(ctx => ctx
            .With(s => s.Id, yamlPipelineStageId));
        _fixture.Customize<PipelineRegistration>(ctx => ctx
            .With(d => d.PipelineId, yamlPipelineId)
            .With(d => d.StageId, yamlPipelineStageId)
            .With(d => d.CiIdentifier, ciIdentifier)
            .With(d => d.PipelineType, pipelineType)
            .With(d => d.ToBeScanned, (bool?)null)
            .With(d => d.PartitionKey, PipelineRegistration.Prod));

        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();
        var classicReleasePipelines = _fixture.CreateMany<ReleaseDefinition>(0).ToList();
        var yamlReleasePipelines = _fixture.CreateMany<BuildDefinition>().ToList();
        var classicBuildPipelines = _fixture.CreateMany<BuildDefinition>().ToList();
        var nonProdPipeline = _fixture.Build<PipelineRegistration>()
            .With(p => p.ToBeScanned, true)
            .With(p => p.PipelineId, _fixture.Create<string>())
            .With(p => p.PartitionKey, PipelineRegistration.NonProd)
            .With(p => p.PipelineType, ItemTypes.YamlReleasePipeline)
            .With(p => p.StageId, (string)null)
            .With(p => p.ProjectId, project.Id).Create();

        registrations.Add(nonProdPipeline);

        _registrationRepoMock
            .Setup(x => x.GetAsync(organization, project.Id))
            .ReturnsAsync(registrations)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetClassicReleasePipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(classicReleasePipelines)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetAllYamlPipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(yamlReleasePipelines)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetClassicBuildPipelinesAsync(organization, project.Id))
            .ReturnsAsync(classicBuildPipelines)
            .Verifiable();
        _scanCiServiceMock
            .Setup(x => x.ScanCiAsync(organization, project, ciIdentifier, scanDate, registrations))
            .ReturnsAsync(ciReport)
            .Verifiable();

        var sut = new ScanProjectService(_azdoClientMock.Object, _registrationRepoMock.Object,
            _pipelinesServiceMock.Object, _scanCiServiceMock.Object,
            _compliancyReportServiceMock.Object, _config);

        // Act
        await sut.ScanProjectAsync(organization, project, scanDate, parallelCiScans);

        // Assert
        _registrationRepoMock.Verify();
        _azdoClientMock.Verify();
        _pipelinesServiceMock.Verify();
        _scanCiServiceMock.Verify();
        _compliancyReportServiceMock
            .Verify(compliancyReportService => compliancyReportService.UpdateComplianceReportAsync(organization,
                Guid.Parse(project.Id), It.Is<CompliancyReport>(x =>
                    x.UnregisteredPipelines.Count == 0 &&
                    x.RegisteredConfigurationItems.Count == 1 &&
                    x.RegisteredPipelines.Count == 1 &&
                    x.RegisteredPipelines.First().Id == yamlPipelineId &&
                    x.RegisteredPipelines.First().Type == "YAML release" &&
                    x.RegisteredPipelines.First().Stages
                        .Select(stageReport => stageReport.Id)
                        .Contains(yamlPipelineStageId) &&
                    x.RegisteredPipelines.First().ExclusionListUrl ==
                    new Uri(
                        $@"https://{_config.OnlineScannerHostName}/api/exclusion-list/{organization}/{project.Id}/{yamlPipelineId}/{ItemTypes.YamlReleasePipeline}") &&
                    x.RegisteredPipelines.First().UpdateRegistrationUrl ==
                    new Uri(
                        $@"https://{_config.OnlineScannerHostName}/api/updateregistration/{organization}/{project.Id}/{yamlPipelineId}/{ItemTypes.YamlReleasePipeline}") &&
                    x.RegisteredPipelines.First().DeleteRegistrationUrl ==
                    new Uri(
                        $@"https://{_config.OnlineScannerHostName}/api/deleteregistration/{organization}/{project.Id}/{yamlPipelineId}/{ItemTypes.YamlReleasePipeline}")),
                scanDate), Times.Once);
    }

    [Fact]
    public async Task ShouldMark_RegisteredClassicPipeline_AsRegistered_AndExecuteScan()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Build<Project>()
            .With(f => f.Id, _fixture.Create<Guid>().ToString)
            .Create();
        var scanDate = _fixture.Create<DateTime>();
        var parallelCiScans = _fixture.Create<int>();
        var classicPipelineId = _fixture.Create<string>();
        var classicPipelineStageId = _fixture.Create<int>();
        var ciIdentifier = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        var ciReport = _fixture.Create<CiReport>();

        _fixture.Customize<ReleaseDefinition>(ctx => ctx
            .With(r => r.Id, classicPipelineId));
        _fixture.Customize<ReleaseDefinitionEnvironment>(ctx => ctx
            .With(e => e.Id, classicPipelineStageId));
        _fixture.Customize<PipelineRegistration>(ctx => ctx
            .With(d => d.PipelineId, classicPipelineId)
            .With(d => d.StageId, classicPipelineStageId.ToString())
            .With(d => d.CiIdentifier, ciIdentifier)
            .With(d => d.PipelineType, pipelineType)
            .With(d => d.ToBeScanned, (bool?)null)
            .With(d => d.PartitionKey, PipelineRegistration.Prod));

        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();
        var classicReleasePipelines = _fixture.CreateMany<ReleaseDefinition>().ToList();
        var yamlReleasePipelines = _fixture.CreateMany<BuildDefinition>(0).ToList();
        var classicBuildPipelines = _fixture.CreateMany<BuildDefinition>().ToList();

        var nonProdPipeline = _fixture.Build<PipelineRegistration>()
            .With(p => p.ToBeScanned, true)
            .With(p => p.PipelineId, _fixture.Create<string>())
            .With(p => p.PartitionKey, PipelineRegistration.NonProd)
            .With(p => p.PipelineType, ItemTypes.ClassicReleasePipeline)
            .With(p => p.StageId, (string)null)
            .With(p => p.ProjectId, project.Id).Create();

        registrations.Add(nonProdPipeline);

        _registrationRepoMock
            .Setup(x => x.GetAsync(organization, project.Id))
            .ReturnsAsync(registrations)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetClassicReleasePipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(classicReleasePipelines)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetAllYamlPipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(yamlReleasePipelines)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetClassicBuildPipelinesAsync(organization, project.Id))
            .ReturnsAsync(classicBuildPipelines)
            .Verifiable();
        _scanCiServiceMock
            .Setup(x => x.ScanCiAsync(organization, project, ciIdentifier, scanDate, registrations))
            .ReturnsAsync(ciReport)
            .Verifiable();

        var sut = new ScanProjectService(_azdoClientMock.Object, _registrationRepoMock.Object,
            _pipelinesServiceMock.Object, _scanCiServiceMock.Object,
            _compliancyReportServiceMock.Object, _config);

        // Act
        await sut.ScanProjectAsync(organization, project, scanDate, parallelCiScans);

        // Assert
        _registrationRepoMock.Verify();
        _azdoClientMock.Verify();
        _pipelinesServiceMock.Verify();
        _scanCiServiceMock.Verify();
        _compliancyReportServiceMock
            .Verify(compliancyReportService => compliancyReportService.UpdateComplianceReportAsync(organization,
                Guid.Parse(project.Id), It.Is<CompliancyReport>(compliancyReport =>
                    compliancyReport.UnregisteredPipelines.Count == 0 &&
                    compliancyReport.RegisteredConfigurationItems.Count == 1 &&
                    compliancyReport.RegisteredPipelines.Count == 1 &&
                    compliancyReport.RegisteredPipelines.First().Id == classicPipelineId &&
                    compliancyReport.RegisteredPipelines.First().Type == "Classic release" &&
                    compliancyReport.RegisteredPipelines.First().Stages
                        .Select(x => x.Id)
                        .Contains(classicPipelineStageId.ToString()) &&
                    compliancyReport.RegisteredPipelines.First().ExclusionListUrl ==
                    new Uri(
                        $@"https://{_config.OnlineScannerHostName}/api/exclusion-list/{organization}/{project.Id}/{classicPipelineId}/{ItemTypes.ClassicReleasePipeline}") &&
                    compliancyReport.RegisteredPipelines.First().UpdateRegistrationUrl ==
                    new Uri(
                        $@"https://{_config.OnlineScannerHostName}/api/updateregistration/{organization}/{project.Id}/{classicPipelineId}/{ItemTypes.ClassicReleasePipeline}") &&
                    compliancyReport.RegisteredPipelines.First().DeleteRegistrationUrl ==
                    new Uri(
                        $@"https://{_config.OnlineScannerHostName}/api/deleteregistration/{organization}/{project.Id}/{classicPipelineId}/{ItemTypes.ClassicReleasePipeline}")),
                scanDate), Times.Once);
    }

    [Fact]
    public async Task ShouldContinue_AfterCiScanFailure_AndMarkFailureInReport()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Build<Project>()
            .With(f => f.Id, _fixture.Create<Guid>().ToString)
            .Create();
        var scanDate = _fixture.Create<DateTime>();
        var parallelCiScans = _fixture.Create<int>();
        var classicPipelineId = _fixture.Create<string>();
        var classicPipelineStageId = _fixture.Create<int>();
        var ciIdentifier = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        var exception = _fixture.Create<Exception>();

        _fixture.Customize<ReleaseDefinition>(ctx => ctx
            .With(r => r.Id, classicPipelineId));
        _fixture.Customize<ReleaseDefinitionEnvironment>(ctx => ctx
            .With(e => e.Id, classicPipelineStageId));
        _fixture.Customize<PipelineRegistration>(ctx => ctx
            .With(d => d.PipelineId, classicPipelineId)
            .With(d => d.StageId, classicPipelineStageId.ToString())
            .With(d => d.CiIdentifier, ciIdentifier)
            .With(d => d.PipelineType, pipelineType)
            .With(d => d.ToBeScanned, (bool?)null)
            .With(d => d.PartitionKey, PipelineRegistration.Prod));

        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();
        var classicReleasePipelines = _fixture.CreateMany<ReleaseDefinition>().ToList();
        var yamlReleasePipelines = _fixture.CreateMany<BuildDefinition>(0).ToList();
        var classicBuildPipelines = _fixture.CreateMany<BuildDefinition>().ToList();

        _registrationRepoMock
            .Setup(x => x.GetAsync(organization, project.Id))
            .ReturnsAsync(registrations)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetClassicReleasePipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(classicReleasePipelines)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetAllYamlPipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(yamlReleasePipelines)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetClassicBuildPipelinesAsync(organization, project.Id))
            .ReturnsAsync(classicBuildPipelines)
            .Verifiable();
        _scanCiServiceMock
            .Setup(x => x.ScanCiAsync(organization, project, ciIdentifier, scanDate, registrations))
            .ThrowsAsync(exception)
            .Verifiable();

        var sut = new ScanProjectService(_azdoClientMock.Object, _registrationRepoMock.Object,
            _pipelinesServiceMock.Object, _scanCiServiceMock.Object,
            _compliancyReportServiceMock.Object, _config);

        // Act
        await sut.ScanProjectAsync(organization, project, scanDate, parallelCiScans);

        // Assert
        _registrationRepoMock.Verify();
        _azdoClientMock.Verify();
        _pipelinesServiceMock.Verify();
        _scanCiServiceMock.Verify();
        _compliancyReportServiceMock
            .Verify(compliancyReportService => compliancyReportService.UpdateComplianceReportAsync(organization,
                Guid.Parse(project.Id), It.Is<CompliancyReport>(compliancyReport =>
                    compliancyReport.UnregisteredPipelines.Count == 0 &&
                    compliancyReport.RegisteredConfigurationItems.Count == 1 &&
                    compliancyReport.RegisteredConfigurationItems.First().IsScanFailed &&
                    compliancyReport.RegisteredConfigurationItems.First().Id == ciIdentifier &&
                    compliancyReport.RegisteredConfigurationItems.First().ScanException.ExceptionMessage ==
                    $"{exception.Message} Stacktrace: {exception.StackTrace}" &&
                    compliancyReport.RegisteredPipelines.Count == 1 &&
                    compliancyReport.RegisteredPipelines.First().Id == classicPipelineId &&
                    compliancyReport.RegisteredPipelines.First().Type == "Classic release" &&
                    compliancyReport.RegisteredPipelines.First().Stages
                        .Select(stageReport => stageReport.Id)
                        .Contains(classicPipelineStageId.ToString())),
                scanDate), Times.Once);
    }

    [Theory]
    [InlineData(ItemTypes.StagelessYamlPipeline)]
    [InlineData(ItemTypes.DisabledYamlPipeline)]
    [InlineData(ItemTypes.InvalidYamlPipeline)]
    public async Task ShouldAdd_NonProd_BuildPipelinesAndRepositories_ToReport(string pipelineType)
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Build<Project>()
            .With(f => f.Id, _fixture.Create<Guid>().ToString)
            .Create();
        var scanDate = _fixture.Create<DateTime>();
        var parallelCiScans = _fixture.Create<int>();

        var registrations = _fixture.CreateMany<PipelineRegistration>(0).ToList();
        var classicReleasePipelines = _fixture.CreateMany<ReleaseDefinition>(0).ToList();
        var yamlReleasePipelines = _fixture.CreateMany<BuildDefinition>(0).ToList();

        _fixture.Customize<BuildDefinition>(ctx => ctx
            .With(x => x.PipelineType, pipelineType));
        var classicBuildPipelines = _fixture.CreateMany<BuildDefinition>().ToList();

        var repositories = _fixture.CreateMany<Repository>().ToList();

        _registrationRepoMock
            .Setup(x => x.GetAsync(organization, project.Id))
            .ReturnsAsync(registrations);
        _pipelinesServiceMock
            .Setup(x => x.GetClassicReleasePipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(classicReleasePipelines);
        _pipelinesServiceMock
            .Setup(x => x.GetAllYamlPipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(yamlReleasePipelines);
        _pipelinesServiceMock
            .Setup(x => x.GetClassicBuildPipelinesAsync(organization, project.Id))
            .ReturnsAsync(classicBuildPipelines);
        _azdoClientMock
            .Setup(x => x.GetAsync(It.IsAny<IEnumerableRequest<Repository>>(), organization))
            .ReturnsAsync(repositories)
            .Verifiable();

        var sut = new ScanProjectService(_azdoClientMock.Object, _registrationRepoMock.Object,
            _pipelinesServiceMock.Object, _scanCiServiceMock.Object,
            _compliancyReportServiceMock.Object, _config);

        // Act
        await sut.ScanProjectAsync(organization, project, scanDate, parallelCiScans);

        // Assert
        _azdoClientMock.Verify();
        _compliancyReportServiceMock
            .Verify(compliancyReportService => compliancyReportService.UpdateComplianceReportAsync(organization,
                Guid.Parse(project.Id), It.Is<CompliancyReport>(compliancyReport =>
                    compliancyReport.BuildPipelines.Count == 1 &&
                    !compliancyReport.BuildPipelines.First().IsProduction &&
                    compliancyReport.BuildPipelines.First().Type == pipelineType &&
                    compliancyReport.Repositories.Count == 1 &&
                    !compliancyReport.Repositories.First().IsProduction &&
                    compliancyReport.Repositories.First().Type == ItemTypes.Repository),
                scanDate), Times.Once);
    }

    [Theory]
    [InlineData(ItemTypes.ClassicBuildPipeline)]
    [InlineData(ItemTypes.StagelessYamlPipeline)]
    public async Task ShouldAdd_ValidProd_BuildPipelines_ToReport(string pipelineType)
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Build<Project>()
            .With(f => f.Id, _fixture.Create<Guid>().ToString)
            .Create();
        var scanDate = _fixture.Create<DateTime>();
        var parallelCiScans = _fixture.Create<int>();
        var yamlPipelineId = _fixture.Create<string>();
        var yamlPipelineStageId = _fixture.Create<string>();
        var buildPipelineId = _fixture.Create<string>();
        var ciIdentifier = _fixture.Create<string>();

        _fixture.Customize<BuildDefinition>(ctx => ctx
            .With(p => p.Id, yamlPipelineId)
            .With(x => x.PipelineType, ItemTypes.YamlPipelineWithStages));
        _fixture.Customize<Stage>(ctx => ctx
            .With(s => s.Id, yamlPipelineStageId));
        _fixture.Customize<PipelineRegistration>(ctx => ctx
            .With(d => d.PipelineId, yamlPipelineId)
            .With(d => d.StageId, yamlPipelineStageId)
            .With(d => d.CiIdentifier, ciIdentifier)
            .With(d => d.ToBeScanned, (bool?)null)
            .With(d => d.PartitionKey, PipelineRegistration.Prod));

        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();
        var classicReleasePipelines = _fixture.CreateMany<ReleaseDefinition>(0).ToList();
        var yamlReleasePipelines = _fixture.CreateMany<BuildDefinition>().ToList();

        _fixture.Customize<CiReport>(ctx => ctx
            .FromFactory<string>(name => new CiReport(ciIdentifier, name, scanDate)));

        _fixture.Customize<ItemReport>(ctx => ctx
            .FromFactory<string>((name) =>
                new ItemReport(buildPipelineId, name, project.Id, scanDate))
            .With(f => f.Type, ItemTypes.BuildPipeline));

        _fixture.Customize<BuildDefinition>(ctx => ctx
            .With(x => x.Id, buildPipelineId)
            .With(x => x.PipelineType, pipelineType));

        var ciReport = _fixture.Create<CiReport>();

        var classicBuildPipelines = _fixture.CreateMany<BuildDefinition>().ToList();
        var buildPipelines = classicBuildPipelines.Concat(yamlReleasePipelines).ToList();

        _registrationRepoMock
            .Setup(x => x.GetAsync(organization, project.Id))
            .ReturnsAsync(registrations);
        _pipelinesServiceMock
            .Setup(x => x.GetClassicReleasePipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(classicReleasePipelines);
        _pipelinesServiceMock
            .Setup(x => x.GetAllYamlPipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(yamlReleasePipelines);
        _pipelinesServiceMock
            .Setup(x => x.GetClassicBuildPipelinesAsync(organization, project.Id))
            .ReturnsAsync(classicBuildPipelines);
        _scanCiServiceMock
            .Setup(x => x.ScanCiAsync(organization, project, ciIdentifier, scanDate, registrations))
            .ReturnsAsync(ciReport);

        var sut = new ScanProjectService(_azdoClientMock.Object, _registrationRepoMock.Object,
            _pipelinesServiceMock.Object, _scanCiServiceMock.Object,
            _compliancyReportServiceMock.Object, _config);

        // Act
        await sut.ScanProjectAsync(organization, project, scanDate, parallelCiScans);

        // Assert
        _compliancyReportServiceMock
            .Verify(x => x.UpdateComplianceReportAsync(organization, Guid.Parse(project.Id), It.Is<CompliancyReport>(
                compliancyReport =>
                    compliancyReport.BuildPipelines.Count == 1 &&
                    compliancyReport.BuildPipelines.First().IsProduction &&
                    compliancyReport.BuildPipelines.First().CiIdentifiers == ciIdentifier &&
                    compliancyReport.BuildPipelines.First().Type == pipelineType &&
                    string.Equals(compliancyReport.BuildPipelines.First().OpenPermissionsUrl.AbsoluteUri,
                        @$"https://{_config.OnlineScannerHostName}/api/open-permissions/{organization}/{project.Id}/{ItemTypes.BuildPipeline}/{buildPipelines.First().Id}",
                        StringComparison.InvariantCultureIgnoreCase)), scanDate), Times.Once);

        _azdoClientMock.Verify();
    }

    [Fact]
    public async Task ShouldAdd_ValidProd_Repository_ToReport()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Build<Project>()
            .With(f => f.Id, _fixture.Create<Guid>().ToString)
            .Create();
        var scanDate = _fixture.Create<DateTime>();
        var parallelCiScans = _fixture.Create<int>();
        var yamlPipelineId = _fixture.Create<string>();
        var yamlPipelineStageId = _fixture.Create<string>();
        var repositoryId = _fixture.Create<string>();
        var ciIdentifier = _fixture.Create<string>();

        _fixture.Customize<BuildDefinition>(ctx => ctx
            .With(p => p.Id, yamlPipelineId)
            .With(x => x.PipelineType, ItemTypes.YamlPipelineWithStages));
        _fixture.Customize<Stage>(ctx => ctx
            .With(s => s.Id, yamlPipelineStageId));
        _fixture.Customize<PipelineRegistration>(ctx => ctx
            .With(d => d.PipelineId, yamlPipelineId)
            .With(d => d.StageId, yamlPipelineStageId)
            .With(d => d.CiIdentifier, ciIdentifier)
            .With(d => d.ToBeScanned, (bool?)null)
            .With(d => d.PartitionKey, PipelineRegistration.Prod));

        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();
        var classicReleasePipelines = _fixture.CreateMany<ReleaseDefinition>(0).ToList();
        var yamlReleasePipelines = _fixture.CreateMany<BuildDefinition>().ToList();
        var classicBuildPipelines = _fixture.CreateMany<BuildDefinition>(0).ToList();

        _fixture.Customize<CiReport>(ctx => ctx
            .FromFactory<string>(name => new CiReport(ciIdentifier, name, scanDate)));

        _fixture.Customize<ItemReport>(ctx => ctx
            .FromFactory<string>((name) =>
                new ItemReport(repositoryId, name, project.Id, scanDate))
            .With(f => f.Type, ItemTypes.Repository));

        _fixture.Customize<Repository>(ctx => ctx
            .With(x => x.Id, repositoryId));

        var ciReport = _fixture.Create<CiReport>();
        var repositories = _fixture.CreateMany<Repository>().ToList();

        _registrationRepoMock
            .Setup(x => x.GetAsync(organization, project.Id))
            .ReturnsAsync(registrations);
        _pipelinesServiceMock
            .Setup(x => x.GetClassicReleasePipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(classicReleasePipelines);
        _pipelinesServiceMock
            .Setup(x => x.GetAllYamlPipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(yamlReleasePipelines);
        _pipelinesServiceMock
            .Setup(x => x.GetClassicBuildPipelinesAsync(organization, project.Id))
            .ReturnsAsync(classicBuildPipelines);
        _scanCiServiceMock
            .Setup(x => x.ScanCiAsync(organization, project, ciIdentifier, scanDate, registrations))
            .ReturnsAsync(ciReport);
        _azdoClientMock
            .Setup(x => x.GetAsync(It.IsAny<IEnumerableRequest<Repository>>(), organization))
            .ReturnsAsync(repositories)
            .Verifiable();

        var sut = new ScanProjectService(_azdoClientMock.Object, _registrationRepoMock.Object,
            _pipelinesServiceMock.Object, _scanCiServiceMock.Object,
            _compliancyReportServiceMock.Object, _config);

        // Act
        await sut.ScanProjectAsync(organization, project, scanDate, parallelCiScans);

        // Assert
        _azdoClientMock.Verify();
        _compliancyReportServiceMock
            .Verify(compliancyReportService => compliancyReportService.UpdateComplianceReportAsync(organization,
                Guid.Parse(project.Id), It.Is<CompliancyReport>(compliancyReport =>
                    compliancyReport.Repositories.Count == 1 &&
                    compliancyReport.Repositories.First().IsProduction &&
                    compliancyReport.Repositories.First().CiIdentifiers == ciIdentifier &&
                    compliancyReport.Repositories.First().Type == ItemTypes.Repository &&
                    string.Equals(compliancyReport.Repositories.First().OpenPermissionsUrl.AbsoluteUri,
                        $@"https://{_config.OnlineScannerHostName}/api/open-permissions/{organization}/{project.Id}/{ItemTypes.Repository}/{repositories.First().Id}",
                        StringComparison.InvariantCultureIgnoreCase)), scanDate), Times.Once);
    }

    [Fact]
    public async Task CreateRegisteredCiReportsAsync_CiScanFails_GetItemIdsFromReport_HandlesNullReference()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var project = _fixture.Build<Project>()
            .With(f => f.Id, _fixture.Create<Guid>().ToString)
            .Create();
        var scanDate = _fixture.Create<DateTime>();
        var parallelCiScans = _fixture.Create<int>();
        var classicPipelineId = _fixture.Create<string>();
        var classicPipelineStageId = _fixture.Create<int>();
        var ciIdentifier = _fixture.Create<string>();
        var ciName = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;

        _fixture.Customize<ReleaseDefinition>(ctx => ctx
            .With(r => r.Id, classicPipelineId));
        _fixture.Customize<ReleaseDefinitionEnvironment>(ctx => ctx
            .With(e => e.Id, classicPipelineStageId));
        _fixture.Customize<PipelineRegistration>(ctx => ctx
            .With(d => d.PipelineId, classicPipelineId)
            .With(d => d.StageId, classicPipelineStageId.ToString())
            .With(d => d.CiIdentifier, ciIdentifier)
            .With(d => d.PipelineType, pipelineType)
            .With(d => d.ToBeScanned, (bool?)null)
            .With(d => d.PartitionKey, PipelineRegistration.Prod));

        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();
        var classicReleasePipelines = _fixture.CreateMany<ReleaseDefinition>().ToList();
        var yamlReleasePipelines = _fixture.CreateMany<BuildDefinition>(0).ToList();
        var classicBuildPipelines = _fixture.CreateMany<BuildDefinition>().ToList();

        _registrationRepoMock
            .Setup(x => x.GetAsync(organization, project.Id))
            .ReturnsAsync(registrations)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetClassicReleasePipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(classicReleasePipelines)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetAllYamlPipelinesAsync(organization, project.Id, registrations))
            .ReturnsAsync(yamlReleasePipelines)
            .Verifiable();
        _pipelinesServiceMock
            .Setup(x => x.GetClassicBuildPipelinesAsync(organization, project.Id))
            .ReturnsAsync(classicBuildPipelines)
            .Verifiable();
        _scanCiServiceMock
            .Setup(x => x.ScanCiAsync(organization, project, ciIdentifier, scanDate, registrations))
            .ReturnsAsync(new CiReport(It.IsAny<string>(), ciName, scanDate)
            {
                IsScanFailed = true,
                RescanUrl = It.IsAny<Uri>(),
                ScanException =
                    new ExceptionSummaryReport(new Exception($"There is no release pipeline for CI: {ciIdentifier}")),
                PrincipleReports = Enumerable.Empty<PrincipleReport>()
            });

        var sut = new ScanProjectService(_azdoClientMock.Object, _registrationRepoMock.Object,
            _pipelinesServiceMock.Object, _scanCiServiceMock.Object,
            _compliancyReportServiceMock.Object, _config);

        // Act
        await sut.ScanProjectAsync(organization, project, scanDate, parallelCiScans);

        // Assert
        _registrationRepoMock.Verify();
        _azdoClientMock.Verify();
        _pipelinesServiceMock.Verify();
        _scanCiServiceMock.Verify();
        _compliancyReportServiceMock
            .Verify(compliancyReportService => compliancyReportService.UpdateComplianceReportAsync(organization,
                Guid.Parse(project.Id), It.Is<CompliancyReport>(compliancyReport =>
                    compliancyReport.UnregisteredPipelines.Count == 0 &&
                    compliancyReport.RegisteredConfigurationItems.Count == 1 &&
                    compliancyReport.RegisteredConfigurationItems.First().IsScanFailed &&
                    compliancyReport.RegisteredPipelines.Count == 1),
                scanDate), Times.Once);
    }
}
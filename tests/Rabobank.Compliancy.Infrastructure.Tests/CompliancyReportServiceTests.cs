using AutoFixture.AutoMoq;
using AutoMapper;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Infrastructure.Config;
using Rabobank.Compliancy.Infrastructure.Constants;
using Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;
using Rabobank.Compliancy.Infrastructure.Mapping;
using System.Linq.Expressions;

namespace Rabobank.Compliancy.Infrastructure.Tests;

public class CompliancyReportServiceTests
{
    private readonly AzureDevOpsExtensionConfig _config = new();
    private readonly Mock<IDeviationService> _deviationServiceMock = new();
    private readonly Mock<IExtensionDataRepository> _extensionDataRepositoryMock = new();
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly IMapper _mapper;
    private readonly CompliancyReportService _sut;

    public CompliancyReportServiceTests()
    {
        _mapper = CreateMapper();
        _sut = new CompliancyReportService(
            _extensionDataRepositoryMock.Object, _config, _mapper, _deviationServiceMock.Object);
    }

    [Fact]
    public async Task UpdateComplianceReportAsync_WithNewDeviationReport_ShouldBeAdded()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var scanDate = _fixture.Create<DateTime>();
        var ciIdentifier = _fixture.Create<string>();
        var ruleName = _fixture.Create<string>();
        var itemId = _fixture.Create<string>();

        var compliancyReport = CreateCompliancyReport(ciIdentifier, ruleName, projectId, itemId);

        var deviation = CreateDeviation(ciIdentifier, ruleName, projectId, itemId);

        _deviationServiceMock.Setup(m => m.GetDeviationsAsync(
                projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { deviation });

        // Act
        await _sut.UpdateComplianceReportAsync(organization, projectId, compliancyReport, scanDate);

        // Assert
        UploadCompliancyReportVerify(organization,
            r => r.Id == compliancyReport.Id && r.RegisteredConfigurationItems!.Single().HasDeviation);

        var expectedDeviationReport = _mapper.Map<DeviationReport>(deviation);

        compliancyReport.RegisteredConfigurationItems!.Single()
            .PrincipleReports!.Single()
            .RuleReports!.Single().ItemReports!.Single()
            .Deviation.Should().BeEquivalentTo(expectedDeviationReport);
    }

    [Fact]
    public async Task UpdateCiReportAsync_WithNewCiReport_ShouldBeUpdated()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var projectName = _fixture.Create<string>();
        var ciIdentifier = _fixture.Create<string>();
        var ruleName = _fixture.Create<string>();
        var itemId = _fixture.Create<string>();
        var newCiReport = CreateCiReport(ciIdentifier, ruleName, projectId, itemId, true);
        var scanDate = _fixture.Create<DateTime>();
        var compliancyReport = CreateCompliancyReport(ciIdentifier, ruleName, projectId, itemId);

        DownloadCompliancyReportSetup(organization, projectName, compliancyReport);

        var deviation = CreateDeviation(ciIdentifier, ruleName, projectId, itemId);

        _deviationServiceMock.Setup(m => m.GetDeviationsAsync(
                projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { deviation });

        // Act
        await _sut.UpdateCiReportAsync(organization, projectId, projectName, newCiReport, scanDate);

        // Assert
        UploadCompliancyReportVerify(organization, r =>
            r.Id == compliancyReport.Id
            && r.RegisteredConfigurationItems!.Single().Id == newCiReport.Id
            && r.RegisteredConfigurationItems!.Single().HasDeviation);
    }

    [Fact]
    public async Task UpdateNonProdPipelineReportAsync_WithNewNonProdCompliancyReport_ShouldBeUpdated()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectName = _fixture.Create<string>();
        var newNonProdCompliancyReport = _fixture.Create<NonProdCompliancyReport>();
        var compliancyReportDto = _fixture.Build<CompliancyReportDto>()
            .With(f => f.NonProdPipelinesRegisteredForScan,
                _fixture.Build<NonProdCompliancyReportDto>()
                    .With(f => f.PipelineId, newNonProdCompliancyReport.PipelineId)
                    .CreateMany(1).ToList)
            .Create();

        DownloadCompliancyReportSetup(organization, projectName, compliancyReportDto);

        // Act
        await _sut.UpdateNonProdPipelineReportAsync(organization, projectName, newNonProdCompliancyReport);

        // Assert
        UploadCompliancyReportVerify(organization, r =>
            r.Id == compliancyReportDto.Id
            && r.NonProdPipelinesRegisteredForScan!.Single().PipelineId == newNonProdCompliancyReport.PipelineId);
    }

    [Fact]
    public async Task UpdateComplianceStatusAsync_WithProjectScope_StatusShouldBeUpdatedForAllItems()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var ciIdentifierOne = _fixture.Create<string>();
        var ciIdentifierTwo = _fixture.Create<string>();
        var ruleName = _fixture.Create<string>();
        var itemIdOne = _fixture.Create<string>();
        var itemIdTwo = _fixture.Create<string>();
        const bool isProjectRule = true;
        const bool isCompliant = true;

        var compliancyReport = CreateCompliancyReport(ciIdentifierOne, ruleName, project.Id, itemIdOne, false);

        compliancyReport.RegisteredConfigurationItems = compliancyReport.RegisteredConfigurationItems!.Concat(
            new[]
            {
                CreateCiReport(ciIdentifierTwo, ruleName, project.Id, itemIdTwo, false)
            }).ToList();

        compliancyReport.NonProdPipelinesRegisteredForScan = compliancyReport.NonProdPipelinesRegisteredForScan!.Concat(
            new[]
            {
                CreateNonProdCompliancyReport(ruleName, project.Id, itemIdTwo, false, false)
            }).ToList();

        DownloadCompliancyReportSetup(project.Organization, project.Name, compliancyReport);

        // Act
        await _sut.UpdateComplianceStatusAsync(project, project.Id, itemIdOne, ruleName, isProjectRule, isCompliant);

        // Assert

        // 2 are made compliant because they belong to itemIdOne.
        UploadCompliancyReportVerify(project.Organization, cr =>
            cr.Id == compliancyReport.Id
            && CountRegisteredConfigurationItemsItemReports(cr, true, false) == 2
            && CountNonProdPipelinesRegisteredForScanItemReports(cr, true, false) == 2);

        // 2 are left not compliant because they belong to itemIdTwo
        UploadCompliancyReportVerify(project.Organization, cr =>
            cr.Id == compliancyReport.Id
            && CountRegisteredConfigurationItemsItemReports(cr, false, false) == 0
            && CountNonProdPipelinesRegisteredForScanItemReports(cr, false, false) == 0);
    }

    /// <summary>
    ///     All rules should be marked as compliant for the specified rule and project over all CI's related to the project.
    /// </summary>
    [Fact]
    public async Task UpdateComplianceStatusAsync_WithItemScope_StatusShouldBeUpdatedForItemIdOnly()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var ciIdentifierOne = _fixture.Create<string>();
        var ciIdentifierTwo = _fixture.Create<string>();
        var ruleName = _fixture.Create<string>();
        var itemIdOne = _fixture.Create<string>();
        var itemIdTwo = _fixture.Create<string>();
        const bool isProjectRule = false;
        const bool isCompliant = true;

        var compliancyReport = CreateCompliancyReport(ciIdentifierOne, ruleName, project.Id, itemIdOne, false);

        compliancyReport.RegisteredConfigurationItems = compliancyReport.RegisteredConfigurationItems!.Concat(
            new[]
            {
                CreateCiReport(ciIdentifierTwo, ruleName, project.Id, itemIdTwo, false)
            }).ToList();

        compliancyReport.NonProdPipelinesRegisteredForScan = compliancyReport.NonProdPipelinesRegisteredForScan!.Concat(
            new[]
            {
                CreateNonProdCompliancyReport(ruleName, project.Id, itemIdTwo, false, false)
            }).ToList();

        DownloadCompliancyReportSetup(project.Organization, project.Name, compliancyReport);

        // Act
        await _sut.UpdateComplianceStatusAsync(project, project.Id, itemIdOne, ruleName, isProjectRule, isCompliant);

        // Assert

        // 2 are made compliant because they belong to itemIdOne.
        UploadCompliancyReportVerify(project.Organization, cr =>
            cr.Id == compliancyReport.Id
            && CountRegisteredConfigurationItemsItemReports(cr, true, false) == 1
            && CountNonProdPipelinesRegisteredForScanItemReports(cr, true, false) == 1);

        // 2 are left not compliant because they belong to itemIdTwo
        UploadCompliancyReportVerify(project.Organization, cr =>
            cr.Id == compliancyReport.Id
            && CountRegisteredConfigurationItemsItemReports(cr, false, false) == 1
            && CountNonProdPipelinesRegisteredForScanItemReports(cr, false, false) == 1);
    }

    [Fact]
    public async Task UpdateComplianceStatusAsync_WithItemScope_ShouldNotUpdateReportsForOtherRules()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var ciIdentifier = _fixture.Create<string>();
        var ruleNameOne = _fixture.Create<string>();
        var ruleNameTwo = _fixture.Create<string>();
        var itemId = _fixture.Create<string>();
        const bool isProjectRule = false;
        const bool isCompliant = true;

        var compliancyReport = CreateCompliancyReport(ciIdentifier, ruleNameOne, project.Id, itemId, false);

        compliancyReport.RegisteredConfigurationItems = compliancyReport.RegisteredConfigurationItems!.Concat(
            new[]
            {
                CreateCiReport(ciIdentifier, ruleNameTwo, project.Id, itemId, false)
            }).ToList();

        compliancyReport.NonProdPipelinesRegisteredForScan = compliancyReport.NonProdPipelinesRegisteredForScan!.Concat(
            new[]
            {
                CreateNonProdCompliancyReport(ruleNameOne, project.Id, itemId, false, false)
            }).ToList();

        DownloadCompliancyReportSetup(project.Organization, project.Name, compliancyReport);

        // Act
        await _sut.UpdateComplianceStatusAsync(project, project.Id, itemId, ruleNameOne, isProjectRule, isCompliant);

        // Assert

        // 3 are made compliant because they belong to ruleNameOne.
        UploadCompliancyReportVerify(project.Organization, cr =>
            cr.Id == compliancyReport.Id
            && CountRegisteredConfigurationItemsItemReports(cr, true, false) == 1
            && CountNonProdPipelinesRegisteredForScanItemReports(cr, true, false) == 2);

        // 1 was left not compliant because it belongs to ruleNameTwo
        UploadCompliancyReportVerify(project.Organization, cr =>
            cr.Id == compliancyReport.Id
            && CountRegisteredConfigurationItemsItemReports(cr, false, false) == 1
            && CountNonProdPipelinesRegisteredForScanItemReports(cr, false, false) == 0);
    }

    [Fact]
    public async Task
        UpdateComplianceStatusAsync_WithItemScope_AndItemIsCrossProjectResource_ShouldNotUpdateReportsForOtherItems()
    {
        // Arrange
        var projectOne = _fixture.Create<Project>();
        var projectIdTwo = _fixture.Create<Guid>();
        var ciIdentifier = _fixture.Create<string>();
        var ruleName = _fixture.Create<string>();
        var itemId = _fixture.Create<string>();
        const bool isProjectRule = false;
        const bool isCompliant = true;

        var compliancyReport = CreateCompliancyReport(ciIdentifier, ruleName, projectOne.Id, itemId, false);

        compliancyReport.RegisteredConfigurationItems = compliancyReport.RegisteredConfigurationItems!.Concat(
            new[]
            {
                CreateCiReport(ciIdentifier, ruleName, projectIdTwo, itemId, false)
            }).ToList();

        compliancyReport.NonProdPipelinesRegisteredForScan = compliancyReport.NonProdPipelinesRegisteredForScan!.Concat(
            new[]
            {
                CreateNonProdCompliancyReport(ruleName, projectOne.Id, itemId, false, false)
            }).ToList();

        DownloadCompliancyReportSetup(projectOne.Organization, projectOne.Name, compliancyReport);

        // Act
        await _sut.UpdateComplianceStatusAsync(projectOne, projectOne.Id, itemId, ruleName, isProjectRule, isCompliant);

        // Assert

        // 3 are made compliant because they belong to projectOne.
        UploadCompliancyReportVerify(projectOne.Organization, cr =>
            cr.Id == compliancyReport.Id
            && CountRegisteredConfigurationItemsItemReports(cr, true, false) == 1
            && CountNonProdPipelinesRegisteredForScanItemReports(cr, true, false) == 2);

        // 1 was left not compliant because it belongs to projectTwo
        UploadCompliancyReportVerify(projectOne.Organization, cr =>
            cr.Id == compliancyReport.Id
            && CountRegisteredConfigurationItemsItemReports(cr, false, false) == 1
            && CountNonProdPipelinesRegisteredForScanItemReports(cr, false, false) == 0);
    }

    [Fact]
    public async Task AddDeviationToReportAsync_ShouldBeAddedToSpecifiedItemReportOnly()
    {
        // Arrange
        var ciIdentifier = _fixture.Create<string>();
        var ruleName = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var itemIdOne = _fixture.Create<string>();
        var itemIdTwo = _fixture.Create<string>();

        var compliancyReport = CreateCompliancyReport(ciIdentifier, ruleName, projectId, itemIdOne);

        compliancyReport.RegisteredConfigurationItems = compliancyReport.RegisteredConfigurationItems!.Concat(
            new[]
            {
                CreateCiReport(ciIdentifier, ruleName, projectId, itemIdTwo, false)
            }).ToList();

        var deviation = CreateDeviation(ciIdentifier, ruleName, projectId, itemIdOne);

        DownloadCompliancyReportSetup(deviation.Project.Organization, deviation.Project.Name, compliancyReport);

        // Act
        await _sut.AddDeviationToReportAsync(deviation);

        // Assert

        // 1 deviation is added to itemIdOne only.
        UploadCompliancyReportVerify(deviation.Project.Organization, cr =>
            cr.Id == compliancyReport.Id
            && CountRegisteredConfigurationItemsItemReports(cr, true, true) == 1
            && CountNonProdPipelinesRegisteredForScanItemReports(cr, true, true) == 0);
    }

    [Fact]
    public async Task AddDeviationToReportAsync_WhenCrossProjectResource_ShouldNotUpdateReportsForOtherItems()
    {
        // Arrange
        var ciIdentifier = _fixture.Create<string>();
        var ruleName = _fixture.Create<string>();
        var projectIdOne = _fixture.Create<Guid>();
        var projectIdTwo = _fixture.Create<Guid>();
        var itemId = _fixture.Create<string>();

        var compliancyReport = CreateCompliancyReport(ciIdentifier, ruleName, projectIdOne, itemId);

        compliancyReport.RegisteredConfigurationItems = compliancyReport.RegisteredConfigurationItems!.Concat(
            new[]
            {
                CreateCiReport(ciIdentifier, ruleName, projectIdTwo, itemId, false)
            }).ToList();

        var deviation = CreateDeviation(ciIdentifier, ruleName, projectIdOne, itemId);

        DownloadCompliancyReportSetup(deviation.Project.Organization, deviation.Project.Name, compliancyReport);

        // Act
        await _sut.AddDeviationToReportAsync(deviation);

        // Assert

        // 1 deviation is added to itemIdOne only.
        UploadCompliancyReportVerify(deviation.Project.Organization, cr =>
            cr.Id == compliancyReport.Id
            && CountRegisteredConfigurationItemsItemReports(cr, true, true) == 1
            && CountNonProdPipelinesRegisteredForScanItemReports(cr, true, true) == 0);
    }

    [Fact]
    public async Task RemoveDeviationFromReportAsync_ShouldBeRemoved()
    {
        // Arrange
        var ciIdentifier = _fixture.Create<string>();
        var ruleName = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var itemId = _fixture.Create<string>();

        var compliancyReport = CreateCompliancyReport(ciIdentifier, ruleName, projectId, itemId, true, true);

        var deviation = CreateDeviation(ciIdentifier, ruleName, projectId, itemId);

        DownloadCompliancyReportSetup(deviation.Project.Organization, deviation.Project.Name, compliancyReport);

        // Act
        await _sut.RemoveDeviationFromReportAsync(deviation);

        // Assert
        UploadCompliancyReportVerify(deviation.Project.Organization,
            r => r.Id == compliancyReport.Id && !r.RegisteredConfigurationItems!.Single().HasDeviation);
    }

    [Fact]
    public async Task UpdateRegistrationAsync_RemovePipelineFromUnregistered()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectName = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        var pipelineType = _fixture.Create<string>();
        var ciIdentifier = _fixture.Create<string>();

        var pipelineReportDto = _fixture.Build<PipelineReportDto>()
            .With(f => f.Id, pipelineId)
            .With(f => f.Type, pipelineType)
            .Create();

        var compliancyReportDto = _fixture.Build<CompliancyReportDto>()
            .With(f => f.UnregisteredPipelines, new[] { pipelineReportDto })
            .Without(f => f.RegisteredPipelines)
            .Without(f => f.RegisteredPipelinesNoProdStage)
            .Create();

        DownloadCompliancyReportSetup(organization, projectName, compliancyReportDto);

        // Act
        await _sut.UpdateRegistrationAsync(organization, projectName, pipelineId, pipelineType, ciIdentifier);

        // Assert
        UploadCompliancyReportVerify(organization,
            r => r.Id == compliancyReportDto.Id
                 && r.UnregisteredPipelines!.Count == 0
                 && r.RegisteredPipelines!.Single().CiIdentifiers!.Contains(ciIdentifier)
                 && r.RegisteredPipelines!.Single().IsProduction == true
                 && r.RegisteredPipelines!.Any(p => p.Id == pipelineId));
    }

    [Fact]
    public async Task UpdateRegistrationAsync_NewRegistrationShouldAddCiIdentifier()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectName = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        var pipelineType = _fixture.Create<string>();
        var ciIdentifierOne = _fixture.Create<string>();
        var ciIdentifierTwo = _fixture.Create<string>();

        var pipelineReportDto = _fixture.Build<PipelineReportDto>()
            .With(f => f.Id, pipelineId)
            .With(f => f.CiIdentifiers, ciIdentifierOne)
            .With(f => f.Type, pipelineType)
            .Create();

        var compliancyReportDto = _fixture.Build<CompliancyReportDto>()
            .With(f => f.UnregisteredPipelines, new[] { pipelineReportDto })
            .Without(f => f.RegisteredPipelines)
            .Without(f => f.RegisteredPipelinesNoProdStage)
            .Create();

        DownloadCompliancyReportSetup(organization, projectName, compliancyReportDto);

        // Act
        await _sut.UpdateRegistrationAsync(organization, projectName, pipelineId, pipelineType, ciIdentifierTwo);

        // Assert
        UploadCompliancyReportVerify(organization,
            r => r.Id == compliancyReportDto.Id
                 && r.UnregisteredPipelines!.Count == 0
                 && r.RegisteredPipelines!.Single().CiIdentifiers!.Contains($"{ciIdentifierOne},{ciIdentifierTwo}")
                 && r.RegisteredPipelines!.Single().IsProduction == true
                 && r.RegisteredPipelines!.Any(p => p.Id == pipelineId));
    }

    [Fact]
    public async Task UnRegisteredPipelineAsync_WithRegisteredPipeline_ShouldUnRegisterTheRegisteredPipeline()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectName = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        var pipelineType = _fixture.Create<string>();

        var ciIdentifier = _fixture.Create<string>();

        var pipelineReportDto = _fixture.Build<PipelineReportDto>()
            .With(f => f.Id, pipelineId)
            .With(f => f.CiIdentifiers, ciIdentifier)
            .With(f => f.Type, pipelineType)
            .Create();

        var compliancyReportDto = _fixture.Build<CompliancyReportDto>()
           .With(f => f.UnregisteredPipelines, Array.Empty<PipelineReportDto>())
           .With(f => f.RegisteredPipelines, new[] { pipelineReportDto })
           .With(f => f.RegisteredPipelinesNoProdStage, Array.Empty<PipelineReportDto>())
           .Create();

        DownloadCompliancyReportSetup(organization, projectName, compliancyReportDto);

        // Act
        await _sut.UnRegisteredPipelineAsync(organization, projectName, pipelineId, pipelineType);

        // Assert
        UploadCompliancyReportVerify(organization,
            r => r.Id == compliancyReportDto.Id
                 && !r.RegisteredPipelines!.Any()
                 && !r.RegisteredPipelinesNoProdStage!.Any()
                 && r.UnregisteredPipelines!.Single().CiIdentifiers!.Contains(ciIdentifier)
                 && r.UnregisteredPipelines!.Any(p => p.Id == pipelineId));
    }

    [Fact]
    public async Task UnRegisteredPipelineAsync_WithNoCompliancyReportFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectName = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        var pipelineType = _fixture.Create<string>();

        // Act
        var actual = () => _sut.UnRegisteredPipelineAsync(organization, projectName, pipelineId, pipelineType);

        // Assert
        await actual.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UnRegisteredPipelineAsync_WithNoPipelineReportFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectName = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        var pipelineType = _fixture.Create<string>();

        var compliancyReportDto = _fixture.Build<CompliancyReportDto>()
           .Without(f => f.UnregisteredPipelines)
           .Without(f => f.RegisteredPipelines)
           .Without(f => f.RegisteredPipelinesNoProdStage)
           .Create();

        DownloadCompliancyReportSetup(organization, projectName, compliancyReportDto);

        // Act
        var actual = () => _sut.UnRegisteredPipelineAsync(organization, projectName, pipelineId, pipelineType);

        // Assert
        await actual.Should().ThrowAsync<InvalidOperationException>();
    }

    private static int CountRegisteredConfigurationItemsItemReports(
    CompliancyReportDto compliancyReport, bool isCompliant, bool hasDeviation)
    {
        var count = compliancyReport.RegisteredConfigurationItems!
            .SelectMany(c => c.PrincipleReports!)
            .SelectMany(p => p.RuleReports!)
            .SelectMany(r => r.ItemReports!)
            .Count(i => i.IsCompliant == isCompliant && i.HasDeviation == hasDeviation);
        return count;
    }

    private static int CountNonProdPipelinesRegisteredForScanItemReports(
        CompliancyReportDto compliancyReport, bool isCompliant, bool hasDeviation)
    {
        var count = compliancyReport.NonProdPipelinesRegisteredForScan!
            .SelectMany(c => c.PrincipleReports!)
            .SelectMany(p => p.RuleReports!)
            .SelectMany(r => r.ItemReports!)
            .Count(i => i.IsCompliant == isCompliant && i.HasDeviation == hasDeviation);
        return count;
    }

    private static IMapper CreateMapper() =>
        new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CompliancyReportMappingProfile>();
            cfg.AddProfile<DeviationMappingProfile>();
        }));

    private void DownloadCompliancyReportSetup(string organization, string projectName,
        CompliancyReport compliancyReport) =>
        DownloadCompliancyReportSetup(organization, projectName, _mapper.Map<CompliancyReportDto>(compliancyReport));

    private void DownloadCompliancyReportSetup(string organization, string projectName,
        CompliancyReportDto compliancyReportDto) =>
        _extensionDataRepositoryMock.Setup(m => m.DownloadAsync<CompliancyReportDto>(
                CompliancyScannerExtensionConstants.Publisher,
                CompliancyScannerExtensionConstants.Collection,
                _config.ExtensionName, organization, projectName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(compliancyReportDto).Verifiable();

    private void UploadCompliancyReportVerify(string organization, Expression<Func<CompliancyReportDto, bool>> match,
        Func<Times>? times = null) =>
        _extensionDataRepositoryMock.Verify(m => m.UploadAsync(
                CompliancyScannerExtensionConstants.Publisher,
                CompliancyScannerExtensionConstants.Collection,
                _config.ExtensionName, organization, It.Is(match), It.IsAny<CancellationToken>()),
            times ?? Times.Once);

    private CompliancyReport CreateCompliancyReport(string ciIdentifier, string ruleName, Guid projectId, string itemId,
        bool isCompliant = true, bool withDeviation = false) =>
        _fixture.Build<CompliancyReport>()
            .With(f => f.RegisteredConfigurationItems,
                new[] { CreateCiReport(ciIdentifier, ruleName, projectId, itemId, isCompliant, withDeviation) })
            .With(f => f.NonProdPipelinesRegisteredForScan,
                new[] { CreateNonProdCompliancyReport(ruleName, projectId, itemId, isCompliant, withDeviation) })
            .Create();

    private NonProdCompliancyReport CreateNonProdCompliancyReport(string ruleName, Guid projectId, string itemId,
        bool isCompliant, bool withDeviation) =>
        _fixture.Build<NonProdCompliancyReport>()
            .With(f => f.PrincipleReports,
                new[] { CreatePrincipleReport(ruleName, projectId, itemId, isCompliant, withDeviation) })
            .Create();

    private PrincipleReport CreatePrincipleReport(string ruleName, Guid projectId, string itemId, bool isCompliant,
        bool withDeviation)
    {
        var scanDate = _fixture.Create<DateTime>();

        _fixture.Customize<PrincipleReport>(c =>
            c.FromFactory<string, bool>((name, isSOx) => new PrincipleReport(name, scanDate)
            {
                HasRulesToCheck = true,
                IsSox = isSOx
            }));

        return _fixture.Build<PrincipleReport>()
            .With(f => f.RuleReports,
                new[] { CreateRuleReport(ruleName, projectId, itemId, isCompliant, withDeviation) })
            .Create();
    }

    private CiReport CreateCiReport(string ciIdentifier, string ruleName, Guid projectId, string itemId,
        bool isCompliant, bool withDeviation = false)
    {
        var ruleReport = CreateRuleReport(ruleName, projectId, itemId, isCompliant, withDeviation);

        var principleReports = _fixture.Build<PrincipleReport>()
            .With(f => f.RuleReports, new[] { ruleReport })
            .CreateMany(1);

        var scanDate = _fixture.Create<DateTime>();

        _fixture.Customize<CiReport>(c =>
            c.FromFactory<string>(name => new CiReport(ciIdentifier, name, scanDate))
                .With(f => f.PrincipleReports, principleReports));

        return _fixture.Create<CiReport>();
    }

    private RuleReport CreateRuleReport(string ruleName, Guid projectId, string itemId, bool isCompliant,
        bool withDeviation)
    {
        var scanDate = _fixture.Create<DateTime>();

        _fixture.Customize<ItemReport>(c =>
            c.FromFactory<string, string, string>((name, type, link) =>
                    new ItemReport(itemId, name, projectId.ToString(), scanDate)
                    {
                        Type = type,
                        Link = link
                    })
                .With(f => f.Deviation, withDeviation ? _fixture.Create<DeviationReport>() : null)
                .Without(f => f.ScanDate)
                .With(f => f.IsCompliantForRule, isCompliant));

        var itemReport = _fixture.CreateMany<ItemReport>(1);

        return _fixture.Build<RuleReport>()
            .FromFactory(() => new RuleReport(ruleName, scanDate))
            .With(f => f.ItemReports, itemReport)
            .Create();
    }

    private Deviation CreateDeviation(string ciIdentifier, string ruleName, Guid projectId, string itemId)
    {
        var project = _fixture.Build<Project>()
            .With(f => f.Id, projectId)
            .Create();

        _fixture.Customize<Deviation>(ctx => ctx
            .FromFactory<string>(comment =>
                new Deviation(itemId, ruleName, ciIdentifier, project, null, null, null, null, comment))
            .Without(f => f.ItemProjectId));

        return _fixture.Create<Deviation>();
    }
}
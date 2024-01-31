#nullable enable

using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Functions.Shared.Tests;
using Shouldly;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class ItemRescanFunctionTests : FunctionRequestTests
{
    private const string ValidGuidAsString = "f185e36c-0ad1-405f-b5d5-99d0c7039457";
    private const string ValidIntAsString = "2022";
    private readonly Mock<IBuildPipelineRuleRescanProcess> _buildPipelineRuleRescanProcessMock = new();
    private readonly Mock<IClassicReleasePipelineRuleRescanProcess> _classicReleasePipelineRuleRescanProcessMock = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly ItemRescanFunction _itemRescanFunctionDefault;
    private readonly Mock<IProjectRuleRescanProcess> _projectRuleRescanProcessMock = new();
    private readonly Mock<IRepositoryRuleRescanProcess> _repositoryRuleRescanProcessMock = new();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();

    private readonly Guid _validGuid = Guid.NewGuid();
    private readonly string _validString;

    public ItemRescanFunctionTests()
    {
        _itemRescanFunctionDefault = new ItemRescanFunction(
            _loggingServiceMock.Object,
            _projectRuleRescanProcessMock.Object,
            _repositoryRuleRescanProcessMock.Object,
            _classicReleasePipelineRuleRescanProcessMock.Object,
            _buildPipelineRuleRescanProcessMock.Object
        );
        _validString = _fixture.Create<string>();
    }

    [Fact]
    public async Task InvalidInput_ShouldReturn_BadRequestObjectResult_WithAggregateException()
    {
        // Arrange
        var organization = string.Empty;
        var projectId = Guid.Empty;
        var itemId = string.Empty;

        // Act
        var result =
            await _itemRescanFunctionDefault.ItemRescan(TestRequest, organization, projectId, null, itemId, null,
                default);

        // Assert
        result.ShouldBeOfType(typeof(BadRequestObjectResult));
        _loggingServiceMock.Verify(x =>
            x.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog,
                It.Is<ExceptionBaseMetaInformation>(e =>
                    e.Function == nameof(ItemRescanFunction) &&
                    e.RequestUrl == TestRequest.RequestUri!.AbsoluteUri &&
                    e.Organization == organization &&
                    e.ProjectId == projectId.ToString()
                ),
                itemId, null, It.IsAny<AggregateException>()
            )
        );
    }

    [Fact]
    public async Task InvalidRuleName_ReturnsInvalidOperationException()
    {
        // Act
        var result = await _itemRescanFunctionDefault
            .ItemRescan(TestRequest, _validString, _validGuid, "NotCorrectRuleName", _validString, null, default);

        // Assert
        result.ShouldBeOfType(typeof(BadRequestObjectResult));
        _loggingServiceMock.Verify(x =>
            x.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog,
                It.IsAny<ExceptionBaseMetaInformation>(),
                It.IsAny<string>(),
                "NotCorrectRuleName",
                It.IsAny<ArgumentException>()
            ));
    }

    [Fact]
    public async Task DummyItem_ShouldReturn_OkResult()
    {
        // Act
        var result = await _itemRescanFunctionDefault
            .ItemRescan(TestRequest, _validString, _validGuid, _validString, ItemTypes.Dummy, null, default);

        // Assert
        result.ShouldBeOfType(typeof(OkResult));
    }

    [Theory]
    [InlineData(1, 0, 0, 0, ValidGuidAsString, RuleNames.NobodyCanDeleteTheProject)]
    [InlineData(0, 1, 0, 0, ValidGuidAsString, RuleNames.NobodyCanDeleteTheRepository)]
    [InlineData(0, 0, 1, 0, ValidIntAsString, RuleNames.NobodyCanDeleteBuilds)]
    [InlineData(0, 0, 1, 0, ValidIntAsString, RuleNames.BuildArtifactIsStoredSecure)]
    [InlineData(0, 0, 1, 0, ValidIntAsString, RuleNames.BuildPipelineHasSonarqubeTask)]
    [InlineData(0, 0, 1, 0, ValidIntAsString, RuleNames.BuildPipelineHasFortifyTask)]
    [InlineData(0, 0, 1, 0, ValidIntAsString, RuleNames.BuildPipelineHasNexusIqTask)]
    [InlineData(0, 0, 1, 0, ValidIntAsString, RuleNames.BuildPipelineHasCredScanTask)]
    [InlineData(0, 0, 1, 0, ValidIntAsString, RuleNames.BuildPipelineFollowsMainframeCobolProcess)]
    [InlineData(0, 0, 0, 1, ValidIntAsString, RuleNames.NobodyCanDeleteReleases)]
    [InlineData(0, 0, 0, 1, ValidIntAsString, RuleNames.NobodyCanManagePipelineGatesAndDeploy)]
    [InlineData(0, 0, 0, 1, ValidIntAsString, RuleNames.ClassicReleasePipelineHasRequiredRetentionPolicy)]
    [InlineData(0, 0, 0, 1, ValidIntAsString, RuleNames.ClassicReleasePipelineUsesBuildArtifact)]
    [InlineData(0, 0, 0, 1, ValidIntAsString, RuleNames.ClassicReleasePipelineHasSm9ChangeTask)]
    [InlineData(0, 0, 0, 1, ValidIntAsString, RuleNames.ClassicReleasePipelineIsBlockedWithout4EyesApproval)]
    [InlineData(0, 0, 0, 1, ValidIntAsString, RuleNames.ClassicReleasePipelineFollowsMainframeCobolReleaseProcess)]
    [InlineData(0, 0, 1, 0, ValidIntAsString, RuleNames.YamlReleasePipelineIsBlockedWithout4EyesApproval)]
    [InlineData(0, 0, 1, 0, ValidIntAsString, RuleNames.YamlReleasePipelineHasRequiredRetentionPolicy)]
    [InlineData(0, 0, 1, 0, ValidIntAsString, RuleNames.YamlReleasePipelineHasSm9ChangeTask)]
    [InlineData(0, 0, 1, 0, ValidIntAsString, RuleNames.YamlReleasePipelineFollowsMainframeCobolReleaseProcess)]
    [InlineData(0, 0, 1, 0, ValidIntAsString, RuleNames.NobodyCanManageEnvironmentGatesAndDeploy)]
    public async Task ValidRuleName_WithParseableItemId_ShouldTriggerCorrectProcess(
        int scanProjectCallCount,
        int scanRepoCallCount,
        int scanBuildPipelineCallCount,
        int scanClassicReleasePipelineCallCount,
        string itemId,
        string ruleName)
    {
        // Act
        var result = await _itemRescanFunctionDefault
            .ItemRescan(TestRequest, _validString, _validGuid, ruleName, itemId, null, default);

        // Assert
        result.ShouldBeOfType(typeof(OkResult));
        _projectRuleRescanProcessMock.Verify(x =>
                x.RescanAndUpdateReportAsync(It.IsAny<ProjectRuleRescanRequest>(), default),
            Times.Exactly(scanProjectCallCount));
        _repositoryRuleRescanProcessMock.Verify(x =>
                x.RescanAndUpdateReportAsync(It.IsAny<RepositoryRuleRescanRequest>(), default),
            Times.Exactly(scanRepoCallCount));
        _buildPipelineRuleRescanProcessMock.Verify(x =>
                x.RescanAndUpdateReportAsync(It.IsAny<PipelineRuleRescanRequest>(), default),
            Times.Exactly(scanBuildPipelineCallCount));
        _classicReleasePipelineRuleRescanProcessMock.Verify(x =>
                x.RescanAndUpdateReportAsync(It.IsAny<PipelineRuleRescanRequest>(), default),
            Times.Exactly(scanClassicReleasePipelineCallCount));
    }

    [Theory]
    [InlineData(ValidGuidAsString, RuleNames.NobodyCanDeleteTheProject)]
    [InlineData(ValidGuidAsString, RuleNames.NobodyCanDeleteTheRepository)]
    [InlineData(ValidIntAsString, RuleNames.NobodyCanDeleteBuilds)]
    [InlineData(ValidIntAsString, RuleNames.BuildArtifactIsStoredSecure)]
    [InlineData(ValidIntAsString, RuleNames.BuildPipelineHasSonarqubeTask)]
    [InlineData(ValidIntAsString, RuleNames.BuildPipelineHasFortifyTask)]
    [InlineData(ValidIntAsString, RuleNames.BuildPipelineHasNexusIqTask)]
    [InlineData(ValidIntAsString, RuleNames.BuildPipelineHasCredScanTask)]
    [InlineData(ValidIntAsString, RuleNames.BuildPipelineFollowsMainframeCobolProcess)]
    [InlineData(ValidIntAsString, RuleNames.NobodyCanDeleteReleases)]
    [InlineData(ValidIntAsString, RuleNames.NobodyCanManagePipelineGatesAndDeploy)]
    [InlineData(ValidIntAsString, RuleNames.ClassicReleasePipelineHasRequiredRetentionPolicy)]
    [InlineData(ValidIntAsString, RuleNames.ClassicReleasePipelineUsesBuildArtifact)]
    [InlineData(ValidIntAsString, RuleNames.ClassicReleasePipelineHasSm9ChangeTask)]
    [InlineData(ValidIntAsString, RuleNames.ClassicReleasePipelineIsBlockedWithout4EyesApproval)]
    [InlineData(ValidIntAsString, RuleNames.ClassicReleasePipelineFollowsMainframeCobolReleaseProcess)]
    [InlineData(ValidIntAsString, RuleNames.YamlReleasePipelineIsBlockedWithout4EyesApproval)]
    [InlineData(ValidIntAsString, RuleNames.YamlReleasePipelineHasRequiredRetentionPolicy)]
    [InlineData(ValidIntAsString, RuleNames.YamlReleasePipelineHasSm9ChangeTask)]
    [InlineData(ValidIntAsString, RuleNames.YamlReleasePipelineFollowsMainframeCobolReleaseProcess)]
    [InlineData(ValidIntAsString, RuleNames.NobodyCanManageEnvironmentGatesAndDeploy)]
    public async Task ItemRescan_WhenExceptionOccurs_ShouldThrowException(string itemId, string ruleName)
    {
        // Arrange
        _repositoryRuleRescanProcessMock
            .Setup(m => m.RescanAndUpdateReportAsync(It.IsAny<RepositoryRuleRescanRequest>(),
                It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();
        _projectRuleRescanProcessMock
            .Setup(m => m.RescanAndUpdateReportAsync(It.IsAny<ProjectRuleRescanRequest>(),
                It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();
        _classicReleasePipelineRuleRescanProcessMock
            .Setup(m => m.RescanAndUpdateReportAsync(It.IsAny<PipelineRuleRescanRequest>(),
                It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();
        _buildPipelineRuleRescanProcessMock
            .Setup(m => m.RescanAndUpdateReportAsync(It.IsAny<PipelineRuleRescanRequest>(),
                It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();

        // Act
        var actual = () => _itemRescanFunctionDefault
            .ItemRescan(TestRequest, _validString, _validGuid, ruleName, itemId, null, default);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(x =>
            x.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog,
                It.IsAny<ExceptionReport>()), Times.Once);
    }

    [Theory]
    [InlineData(ValidIntAsString, RuleNames.NobodyCanDeleteTheProject)]
    [InlineData(ValidIntAsString, RuleNames.NobodyCanDeleteTheRepository)]
    [InlineData(ValidGuidAsString, RuleNames.NobodyCanDeleteBuilds)]
    [InlineData(ValidGuidAsString, RuleNames.ClassicReleasePipelineHasSm9ChangeTask)]
    [InlineData(ValidGuidAsString, RuleNames.YamlReleasePipelineHasSm9ChangeTask)]
    public async Task ValidRuleName_WithoutParseableItemId_ShouldNotCallAnyProcess_AndReturnBadRequestObjectResult(
        string itemId,
        string ruleName
    )
    {
        // Act
        var result = await _itemRescanFunctionDefault
            .ItemRescan(TestRequest, _validString, _validGuid, ruleName, itemId, null, default);

        // Assert
        result.ShouldBeOfType(typeof(BadRequestObjectResult));
        _projectRuleRescanProcessMock.Verify(x =>
                x.RescanAndUpdateReportAsync(It.IsAny<ProjectRuleRescanRequest>(), default),
            Times.Never);
        _repositoryRuleRescanProcessMock.Verify(x =>
                x.RescanAndUpdateReportAsync(It.IsAny<RepositoryRuleRescanRequest>(), default),
            Times.Never);
        _buildPipelineRuleRescanProcessMock.Verify(x =>
                x.RescanAndUpdateReportAsync(It.IsAny<PipelineRuleRescanRequest>(), default),
            Times.Never);
        _classicReleasePipelineRuleRescanProcessMock.Verify(x =>
                x.RescanAndUpdateReportAsync(It.IsAny<PipelineRuleRescanRequest>(), default),
            Times.Never);
    }

    [Fact]
    public void AllRules_MustHave_AnItemRescanEndpoint()
    {
        var ruleInterface = typeof(IRule);
        var allRuleNames = ruleInterface.Assembly.GetTypes()
            .Where(implementingRule =>
                ruleInterface.IsAssignableFrom(implementingRule) && !implementingRule.IsInterface)
            .Select(implementingRule => implementingRule.Name);

        var functionNames = typeof(ItemRescanFunction)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Select(publicFunction => publicFunction.GetAttribute<FunctionNameAttribute>())
            .Where(functionNameAttribute => functionNameAttribute != null)
            .Select(functionNameAttribute => functionNameAttribute.Name).ToList();

        foreach (var ruleName in allRuleNames)
            if (!functionNames.Any(functionName =>
                    functionName.Contains(ruleName, StringComparison.InvariantCultureIgnoreCase)))
            {
                Assert.Fail($"RuleName {ruleName} does not have a corresponding ItemRescan function.");
            }
    }
}
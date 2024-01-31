#nullable enable

using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.InputValidation.Services;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Functions.PipelineBreaker.Model;
using Rabobank.Compliancy.Functions.PipelineBreaker.Services;
using Rabobank.Compliancy.Functions.Shared.Tests;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Xunit;
using static Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model.Constants;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.PipelineBreaker.Tests;

public class PipelineBreakerFunctionTests : FunctionRequestTests
{
    private readonly Mock<ILoggingService> _loggingServiceMock = new();
    private readonly Mock<IValidateInputService> _validateInputServiceMock = new();
    private readonly Mock<IPipelineBreakerService> _pipelineBreakerServiceMock = new();
    private readonly Mock<IPipelineRegistrationRepository> _registrationRepositoryMock = new();
    private readonly Mock<IExclusionStorageRepository> _exclusionRepositoryMock = new();

    private readonly IFixture _fixture = new Fixture();

    private readonly PipelineBreakerConfig _configWithBlocking =
        new()
        {
            BlockUnregisteredPipelinesEnabled = true,
            BlockIncompliantPipelinesEnabled = true,
            ThrowWarningsIncompliantPipelinesEnabled = true
        };

    private readonly PipelineBreakerConfig _configWithoutBlocking =
        new()
        {
            BlockUnregisteredPipelinesEnabled = false,
            BlockIncompliantPipelinesEnabled = false,
            ThrowWarningsIncompliantPipelinesEnabled = true
        };

    [Fact]
    public async Task RunAsync_InvalidInput_ReturnsHttpOkAndLogsException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var runId = _fixture.Create<string>();
        var pipelineType = _fixture.Create<string>();
        var stageId = _fixture.Create<string>();
        var exception = _fixture.Create<Exception>();
        var functionName = nameof(PipelineBreakerFunction);

        _validateInputServiceMock
            .Setup(m => m.Validate(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HttpRequestMessage>()))
            .Verifiable();
        _validateInputServiceMock
            .Setup(m => m.ValidateItemType(It.IsAny<string>(), It.IsAny<string[]>()))
            .Throws(exception)
            .Verifiable();

        var function = new PipelineBreakerFunction(_configWithBlocking, _validateInputServiceMock.Object,
            _pipelineBreakerServiceMock.Object, _registrationRepositoryMock.Object, _exclusionRepositoryMock.Object,
            _loggingServiceMock.Object);

        // Act
        var result = await function.RunAsync(TestRequest, organization, projectId, runId, stageId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(OkObjectResult));
        ((ObjectResult)result).Value?.ToString().ShouldStartWith($"{DecoratorErrors.ErrorPrefix}An internal server error occurred");

        _loggingServiceMock.Verify(item => item.LogExceptionAsync(LogDestinations.PipelineBreakerErrorLog,
            exception, It.Is<ExceptionBaseMetaInformation>(e =>
                e.Function == functionName &&
                e.RequestUrl == TestRequest.RequestUri!.AbsoluteUri &&
                e.Organization == organization &&
                e.ProjectId == projectId),
            null, pipelineType)
        );
    }

    [Theory]
    [InlineData(PipelineBreakerResult.None, 1, $"{DecoratorErrors.ErrorPrefix}An internal server error occurred")]
    [InlineData(PipelineBreakerResult.Passed, 0, DecoratorResultMessages.AlreadyScanned)]
    [InlineData(PipelineBreakerResult.Warned, 0, DecoratorResultMessages.WarningAlreadyScanned)]
    [InlineData(PipelineBreakerResult.Blocked, 1, $"{DecoratorErrors.ErrorPrefix}An internal server error occurred")]
    public async Task RunAsync_AlreadyScanned_ReturnsHttpOkAndStopsScan(
        PipelineBreakerResult previousScanResult, int expectedCount, string resultMessage)
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var runId = _fixture.Create<string>();
        var pipelineType = _fixture.Create<string>();
        var stageId = _fixture.Create<string>();
        var exception = _fixture.Create<Exception>();

        _pipelineBreakerServiceMock
            .Setup<System.Threading.Tasks.Task<PipelineBreakerRegistrationReport>>(m => m.GetPreviousRegistrationResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerRegistrationReport { Result = previousScanResult, RegistrationStatus = "NON-PROD" })
            .Verifiable();
        _pipelineBreakerServiceMock
            .Setup(m => m.EnrichPipelineInfoAsync(It.IsAny<PipelineRunInfo>()))
            .Throws(exception);

        var function = new PipelineBreakerFunction(_configWithoutBlocking, _validateInputServiceMock.Object,
            _pipelineBreakerServiceMock.Object, _registrationRepositoryMock.Object, _exclusionRepositoryMock.Object,
            _loggingServiceMock.Object);

        // Act
        var result = await function.RunAsync(request, organization, projectId, runId, stageId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(OkObjectResult));
        ((ObjectResult)result).Value!.ToString().ShouldStartWith(resultMessage);

        _pipelineBreakerServiceMock.Verify();
        _pipelineBreakerServiceMock
            .Verify(m => m.EnrichPipelineInfoAsync(It.IsAny<PipelineRunInfo>()), Times.Exactly(expectedCount));
    }

    #region [registration]

    [Theory]
    [InlineData("PROD", "StageId", "WARNING: This pipeline run has already been scanned during a previous job")]
    [InlineData("NON-PROD", "StageId", "This pipeline is allowed to continue")]
    [InlineData("PROD", "OtherStage", "WARNING: Your pipeline is registered as a PROD pipeline and none of the PROD stages registered")]
    public async Task RunAsync_IsRegistered_ReturnsHttpOk(string partitionKey, string stageId, string resultMessage)
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var runId = _fixture.Create<string>();
        var pipelineType = _fixture.Create<string>();
        var previousScanResult = PipelineBreakerResult.None;

        var runInfo = _fixture.Build<PipelineRunInfo>()
            .With(r => r.Stages, new List<StageReport> { new() { Id = "StageId", Name = "StageName" } })
            .Create();

        var pipelineRegistrations = _fixture.Build<PipelineRegistration>()
            .With(x => x.PartitionKey, partitionKey)
            .With(x => x.StageId, stageId)
            .CreateMany(1)
            .ToList();

        _pipelineBreakerServiceMock
            .Setup<System.Threading.Tasks.Task<PipelineBreakerRegistrationReport>>(m => m.GetPreviousRegistrationResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerRegistrationReport { Result = previousScanResult });
        _pipelineBreakerServiceMock
            .Setup(m => m.EnrichPipelineInfoAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(runInfo);
        _registrationRepositoryMock
            .Setup(m => m.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(pipelineRegistrations)
            .Verifiable();
        _pipelineBreakerServiceMock
            .Setup(m => m.GetPreviousComplianceResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerReport { Result = PipelineBreakerResult.Warned });

        var function = new PipelineBreakerFunction(_configWithoutBlocking, _validateInputServiceMock.Object,
            _pipelineBreakerServiceMock.Object, _registrationRepositoryMock.Object, _exclusionRepositoryMock.Object,
            _loggingServiceMock.Object);

        // Act
        var result = await function.RunAsync(request, organization, projectId, runId, stageId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(OkObjectResult));
        ((ObjectResult)result).Value!.ToString().ShouldStartWith(resultMessage);

        _registrationRepositoryMock.Verify();
    }

    [Theory]
    [InlineData(ItemTypes.ClassicBuildPipeline, "This pipeline is allowed to continue")]
    [InlineData(ItemTypes.StagelessYamlPipeline, "This pipeline is allowed to continue")]
    [InlineData(ItemTypes.YamlPipelineWithStages, "This pipeline is allowed to continue")]
    [InlineData(ItemTypes.YamlReleasePipeline, "ERROR: This pipeline is not registered")]
    [InlineData(ItemTypes.ClassicReleasePipeline, "ERROR: This pipeline is not registered")]
    public async Task RunAsync_UnscannablePipelineType_ReturnsHttpOk(string foundPipelineType, string resultMessage)
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var runId = _fixture.Create<string>();
        var pipelineType = _fixture.Create<string>();
        var stageId = _fixture.Create<string>();
        var previousScanResult = PipelineBreakerResult.None;
        var runInfo = _fixture.Build<PipelineRunInfo>()
            .With(r => r.PipelineType, foundPipelineType)
            .Create();

        _pipelineBreakerServiceMock
            .Setup<System.Threading.Tasks.Task<PipelineBreakerRegistrationReport>>(m => m.GetPreviousRegistrationResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerRegistrationReport { Result = previousScanResult });
        _pipelineBreakerServiceMock
            .Setup(m => m.EnrichPipelineInfoAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(runInfo);
        _registrationRepositoryMock
            .Setup(m => m.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(new List<PipelineRegistration>());

        var function = new PipelineBreakerFunction(_configWithBlocking, _validateInputServiceMock.Object,
            _pipelineBreakerServiceMock.Object, _registrationRepositoryMock.Object, _exclusionRepositoryMock.Object,
            _loggingServiceMock.Object);

        // Act
        var result = await function.RunAsync(request, organization, projectId, runId, stageId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(OkObjectResult));
        ((ObjectResult)result).Value!.ToString().ShouldStartWith(resultMessage);

        _pipelineBreakerServiceMock.Verify();
    }

    [Theory]
    [InlineData(true, "ERROR", PipelineBreakerResult.Blocked)]
    [InlineData(false, "WARNING", PipelineBreakerResult.Warned)]
    public async Task RunAsync_UnregisteredPipeline_ReturnsWarningOrErrorAndLogsReport(
        bool blockingEnabled, string resultMessage, PipelineBreakerResult scanResult)
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var runId = _fixture.Create<string>();
        var pipelineType = _fixture.Create<string>();
        var stageId = _fixture.Create<string>();
        var previousScanResult = PipelineBreakerResult.None;
        var runInfo = _fixture.Build<PipelineRunInfo>()
            .With(r => r.PipelineType, ItemTypes.ClassicReleasePipeline)
            .Create();
        var config = blockingEnabled
            ? _configWithBlocking
            : _configWithoutBlocking;

        _pipelineBreakerServiceMock
            .Setup<System.Threading.Tasks.Task<PipelineBreakerRegistrationReport>>(m => m.GetPreviousRegistrationResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerRegistrationReport { Result = previousScanResult });
        _pipelineBreakerServiceMock
            .Setup(m => m.EnrichPipelineInfoAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(runInfo);
        _registrationRepositoryMock
            .Setup(m => m.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(new List<PipelineRegistration>());

        var function = new PipelineBreakerFunction(config, _validateInputServiceMock.Object,
            _pipelineBreakerServiceMock.Object, _registrationRepositoryMock.Object, _exclusionRepositoryMock.Object,
            _loggingServiceMock.Object);

        // Act
        var result = await function.RunAsync(request, organization, projectId, runId, stageId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(OkObjectResult));
        ((ObjectResult)result).Value!.ToString().ShouldStartWith(resultMessage);

        _loggingServiceMock
            .Verify(m => m.LogInformationAsync(LogDestinations.PipelineBreakerLog,
                It.Is<PipelineBreakerRegistrationReport>(x =>
                    x.RegistrationStatus == null &&
                    x.PipelineType == runInfo.PipelineType &&
                    x.Result == scanResult &&
                    x.Organization == runInfo.Organization &&
                    x.ProjectId == runInfo.ProjectId)));
    }

    [Theory]
    [InlineData("WARNING: Your pipeline is registered as a PROD pipeline and none of the PROD stages registered in the CMDB are present")]
    public async Task RunAsync_NoProdStageOnNonDefaultBranch_ReturnsHttpOkWithErrorMessage(string resultMessage)
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var runId = _fixture.Create<string>();
        var pipelineType = ItemTypes.ReleasePipeline;
        var stageId = "BuildStage";
        var previousScanResult = PipelineBreakerResult.None;

        var runInfo = _fixture.Build<PipelineRunInfo>()
            .With(r => r.Stages, new List<StageReport> { new() { Id = stageId } })
            .Create();

        var pipelineRegistrations = _fixture.Build<PipelineRegistration>()
            .With(x => x.PartitionKey, PipelineRegistration.Prod)
            .With(x => x.StageId, "RegisteredProdStage")
            .CreateMany(1)
            .ToList();

        _pipelineBreakerServiceMock
            .Setup<System.Threading.Tasks.Task<PipelineBreakerRegistrationReport>>(m => m.GetPreviousRegistrationResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerRegistrationReport { Result = previousScanResult });
        _pipelineBreakerServiceMock
            .Setup(m => m.EnrichPipelineInfoAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(runInfo);
        _registrationRepositoryMock
            .Setup(m => m.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(pipelineRegistrations);

        var function = new PipelineBreakerFunction(_configWithBlocking, _validateInputServiceMock.Object,
            _pipelineBreakerServiceMock.Object, _registrationRepositoryMock.Object, _exclusionRepositoryMock.Object,
            _loggingServiceMock.Object);

        // Act
        var result = await function.RunAsync(request, organization, projectId, runId, stageId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(OkObjectResult));
        ((ObjectResult)result).Value!.ToString().ShouldStartWith(resultMessage);

        _loggingServiceMock.Verify(m => m.LogExceptionAsync(It.IsAny<LogDestinations>(), It.IsAny<ExceptionBaseMetaInformation>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
    }

    #endregion [registration]

    #region [compliancy]

    [Fact]
    public async Task RunAsync_HasRuleReportWithDeviation_MultipleRules_ReturnsHttpOkWithCorrectMessage()
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var runId = _fixture.Create<string>();
        var pipelineType = ItemTypes.ReleasePipeline;
        var stageId = _fixture.Create<string>();
        var previousScanResult = PipelineBreakerResult.None;
        var runInfo = _fixture.Create<PipelineRunInfo>();
        runInfo.Stages = new List<StageReport> { new() { Id = "prod" } };

        var pipelineRegistrations = _fixture.Build<PipelineRegistration>()
            .With(x => x.PartitionKey, PipelineRegistration.Prod)
            .With(x => x.StageId, "prod")
            .CreateMany(1)
            .ToList();

        IRule retentionRule = new ClassicReleasePipelineHasRequiredRetentionPolicy(null);
        IRule gatesRule = new NobodyCanManagePipelineGatesAndDeploy(null, null, null);
        var ruleReports = new List<RuleCompliancyReport>()
        {
            new()
            {
                HasDeviation = true,
                IsCompliant = false,
                RuleDescription = retentionRule.Description
            },
            new()
            {
                HasDeviation = true,
                IsCompliant = false,
                RuleDescription = gatesRule.Description
            }
        };

        _pipelineBreakerServiceMock
            .Setup<System.Threading.Tasks.Task<PipelineBreakerRegistrationReport>>(m => m.GetPreviousRegistrationResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerRegistrationReport { Result = previousScanResult });
        _pipelineBreakerServiceMock
            .Setup(m => m.EnrichPipelineInfoAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(runInfo);
        _registrationRepositoryMock
            .Setup(m => m.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(pipelineRegistrations);
        _pipelineBreakerServiceMock
            .Setup(m => m.GetPreviousComplianceResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerReport { Result = previousScanResult });
        _exclusionRepositoryMock
            .Setup(m => m.GetExclusionAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync((Exclusion?)null);
        _pipelineBreakerServiceMock
            .Setup(m => m.GetCompliancy(It.IsAny<PipelineRunInfo>(), It.IsAny<IList<PipelineRegistration>>()))
            .ReturnsAsync(ruleReports);

        var function = new PipelineBreakerFunction(_configWithBlocking, _validateInputServiceMock.Object,
            _pipelineBreakerServiceMock.Object, _registrationRepositoryMock.Object, _exclusionRepositoryMock.Object,
            _loggingServiceMock.Object);

        // Act
        var result = await function.RunAsync(request, organization, projectId, runId, stageId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(OkObjectResult));
        var message = ((ObjectResult)result).Value!.ToString();
        message.ShouldBe(DecoratorResultMessages.Passed);

        _exclusionRepositoryMock.Verify();
    }

    [Theory]
    [InlineData(true, PipelineBreakerResult.Passed, DecoratorResultMessages.ExclusionList)]
    [InlineData(false, PipelineBreakerResult.Blocked, DecoratorResultMessages.NotCompliant)]
    public async Task RunAsync_HasRuleReportWithValidExclusion_ReturnsHttpOkWithCorrectMessage(bool isValidExclusion,
        PipelineBreakerResult pipelineBreakerResult, string resultMessage)
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var runId = _fixture.Create<string>();
        var pipelineType = ItemTypes.ReleasePipeline;
        var stageId = _fixture.Create<string>();
        var previousScanResult = PipelineBreakerResult.None;
        var runInfo = _fixture.Build<PipelineRunInfo>()
            .With(p => p.RunId, runId)
            .With(p => p.Stages, new List<StageReport> { new() { Id = "prod" } })
            .Create();

        var pipelineRegistrations = _fixture.Build<PipelineRegistration>()
            .With(x => x.PartitionKey, PipelineRegistration.Prod)
            .With(x => x.StageId, "prod")
            .CreateMany(1)
            .ToList();

        var ruleReports = new List<RuleCompliancyReport>()
        {
            new()
            {
                HasDeviation = false,
                IsCompliant = false,
                RuleDescription = _fixture.Create<string>()
            }
        };

        var exclusion = _fixture.Build<Exclusion>()
            .With(e => e.Requester, "requester")
            .With(e => e.ExclusionReasonRequester, "I need it")
            .With(e => e.Approver, "approver")
            .With(e => e.ExclusionReasonApprover, "I need it too!")
            .With(e => e.RunId, runId)
            .With(e => e.Timestamp, isValidExclusion ? DateTime.Now.AddHours(-10) : DateTime.Now.AddHours(-30))
            .Create();

        _pipelineBreakerServiceMock
            .Setup<System.Threading.Tasks.Task<PipelineBreakerRegistrationReport>>(m => m.GetPreviousRegistrationResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerRegistrationReport { Result = previousScanResult });
        _pipelineBreakerServiceMock
            .Setup(m => m.EnrichPipelineInfoAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(runInfo);
        _registrationRepositoryMock
            .Setup(m => m.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(pipelineRegistrations);
        _pipelineBreakerServiceMock
            .Setup(m => m.GetPreviousComplianceResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerReport { Result = previousScanResult });
        _exclusionRepositoryMock
            .Setup(m => m.GetExclusionAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(exclusion);
        _pipelineBreakerServiceMock
            .Setup(m => m.GetCompliancy(It.IsAny<PipelineRunInfo>(), It.IsAny<IList<PipelineRegistration>>()))
            .ReturnsAsync(ruleReports);

        var function = new PipelineBreakerFunction(_configWithBlocking, _validateInputServiceMock.Object,
            _pipelineBreakerServiceMock.Object, _registrationRepositoryMock.Object, _exclusionRepositoryMock.Object,
            _loggingServiceMock.Object);

        // Act
        var result = await function.RunAsync(request, organization, projectId, runId, stageId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(OkObjectResult));
        var message = ((ObjectResult)result).Value!.ToString();
        message.ShouldStartWith(resultMessage);

        _exclusionRepositoryMock.Verify();

        _loggingServiceMock
            .Verify(m => m.LogInformationAsync(LogDestinations.PipelineBreakerComplianceLog,
                It.Is<PipelineBreakerReport>(x =>
                    x.Result == pipelineBreakerResult &&
                    x.IsExcluded == isValidExclusion &&
                    x.Requester == "requester" &&
                    x.ExclusionReasonRequester == "I need it" &&
                    x.Approver == "approver" &&
                    x.ExclusionReasonApprover == "I need it too!")));
    }

    [Theory]
    [InlineData(PipelineBreakerResult.Passed, true, DecoratorResultMessages.AlreadyScanned)]
    [InlineData(PipelineBreakerResult.Passed, false, DecoratorResultMessages.AlreadyScanned)]
    [InlineData(PipelineBreakerResult.Warned, false, DecoratorResultMessages.WarningAlreadyScanned)]
    public async Task RunAsync_AlreadyScannedForCompliancy_ReturnsCorrectMessage(PipelineBreakerResult previousCompliancyResult, bool exclusion,
        string resultMessage)
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var runId = _fixture.Create<string>();
        var pipelineType = ItemTypes.ReleasePipeline;
        var stageId = _fixture.Create<string>();
        var previousScanResult = PipelineBreakerResult.None;
        var runInfo = _fixture.Create<PipelineRunInfo>();
        runInfo.Stages = new List<StageReport> { new() { Id = "prod" } };

        var pipelineRegistrations = _fixture.Build<PipelineRegistration>()
            .With(x => x.PartitionKey, PipelineRegistration.Prod)
            .With(x => x.StageId, "prod")
            .CreateMany(1)
            .ToList();

        var ruleReports = new List<RuleCompliancyReport>()
        {
            new()
            {
                HasDeviation = false,
                IsCompliant = true,
                RuleDescription = _fixture.Create<string>()
            }
        };

        _pipelineBreakerServiceMock
            .Setup<System.Threading.Tasks.Task<PipelineBreakerRegistrationReport>>(m => m.GetPreviousRegistrationResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerRegistrationReport { Result = previousScanResult });
        _pipelineBreakerServiceMock
            .Setup(m => m.EnrichPipelineInfoAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(runInfo);
        _registrationRepositoryMock
            .Setup(m => m.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(pipelineRegistrations);
        _pipelineBreakerServiceMock
            .Setup(m => m.GetPreviousComplianceResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerReport
            {
                Result = previousCompliancyResult,
                IsExcluded = exclusion,
                RuleCompliancyReports = ruleReports
            });

        var function = new PipelineBreakerFunction(_configWithoutBlocking, _validateInputServiceMock.Object,
            _pipelineBreakerServiceMock.Object, _registrationRepositoryMock.Object, _exclusionRepositoryMock.Object,
            _loggingServiceMock.Object);

        // Act
        var result = await function.RunAsync(request, organization, projectId, runId, stageId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(OkObjectResult));
        var message = ((ObjectResult)result).Value!.ToString();
        message.ShouldStartWith(resultMessage);
    }

    [Fact]
    public async Task RunAsync_ValidExclusionPresent_RunIdIsUpdated()
    {
        // Arrange
        var runId = _fixture.Create<string>();
        var runInfo = _fixture.Build<PipelineRunInfo>()
            .With(p => p.RunId, runId)
            .With(p => p.Stages, new List<StageReport> { new() { Id = "prod" } })
            .Create();

        var pipelineRegistrations = _fixture.Build<PipelineRegistration>()
            .With(x => x.PartitionKey, PipelineRegistration.Prod)
            .With(x => x.StageId, "prod")
            .CreateMany(1)
            .ToList();

        var ruleReports = new List<RuleCompliancyReport>();

        var exclusion = _fixture.Build<Exclusion>()
            .With(e => e.Timestamp, DateTime.Now.AddHours(-10))
            .Create();

        exclusion.RunId = null;

        _pipelineBreakerServiceMock
            .Setup<System.Threading.Tasks.Task<PipelineBreakerRegistrationReport>>(m => m.GetPreviousRegistrationResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerRegistrationReport { Result = PipelineBreakerResult.None });
        _pipelineBreakerServiceMock
            .Setup(m => m.EnrichPipelineInfoAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(runInfo);
        _registrationRepositoryMock
            .Setup(m => m.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(pipelineRegistrations);
        _pipelineBreakerServiceMock
            .Setup(m => m.GetPreviousComplianceResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerReport
            {
                Result = PipelineBreakerResult.None,
                IsExcluded = false,
                RuleCompliancyReports = ruleReports
            });
        _exclusionRepositoryMock
            .Setup(m => m.GetExclusionAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(exclusion);

        var function = new PipelineBreakerFunction(_configWithBlocking, _validateInputServiceMock.Object,
            _pipelineBreakerServiceMock.Object, _registrationRepositoryMock.Object, _exclusionRepositoryMock.Object,
            _loggingServiceMock.Object);

        // Act
        await function.RunAsync(new HttpRequestMessage(), "raboweb-test", "tas", runId, "prod", ItemTypes.ReleasePipeline);

        // Assert
        _exclusionRepositoryMock.Verify(m => m.SetRunIdAsync(It.Is<PipelineRunInfo>(r => r.RunId == runId)), Times.Once);
    }

    [Fact]
    public async Task RunAsync_InValidExclusionPresent_RunIdIsNotUpdated()
    {
        // Arrange
        var runId = _fixture.Create<string>();
        var runInfo = _fixture.Build<PipelineRunInfo>()
            .With(p => p.RunId, runId)
            .Create();

        var pipelineRegistrations = _fixture.Build<PipelineRegistration>()
            .With(x => x.PartitionKey, PipelineRegistration.Prod)
            .CreateMany(1)
            .ToList();

        var ruleReports = new List<RuleCompliancyReport>();

        var exclusion = _fixture.Build<Exclusion>()
            .With(e => e.Timestamp, DateTime.Now.AddHours(-30))
            .Create();

        exclusion.RunId = null;

        _pipelineBreakerServiceMock
            .Setup<System.Threading.Tasks.Task<PipelineBreakerRegistrationReport>>(m => m.GetPreviousRegistrationResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerRegistrationReport { Result = PipelineBreakerResult.None });
        _pipelineBreakerServiceMock
            .Setup(m => m.EnrichPipelineInfoAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(runInfo);
        _registrationRepositoryMock
            .Setup(m => m.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(pipelineRegistrations);
        _pipelineBreakerServiceMock
            .Setup(m => m.GetPreviousComplianceResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerReport
            {
                Result = PipelineBreakerResult.None,
                IsExcluded = false,
                RuleCompliancyReports = ruleReports
            });
        _exclusionRepositoryMock
            .Setup(m => m.GetExclusionAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(exclusion);

        var function = new PipelineBreakerFunction(_configWithBlocking, _validateInputServiceMock.Object,
            _pipelineBreakerServiceMock.Object, _registrationRepositoryMock.Object, _exclusionRepositoryMock.Object,
            _loggingServiceMock.Object);

        // Act
        await function.RunAsync(new HttpRequestMessage(), "raboweb-test", "tas", runId, "prod", ItemTypes.ReleasePipeline);

        // Assert
        _exclusionRepositoryMock.Verify(m => m.SetRunIdAsync(It.Is<PipelineRunInfo>(r => r.RunId == runId)), Times.Never);
    }

    [Fact]
    public async Task RunAsync_OverallBlockingEnabled_ProjectIsBlocked()
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var runId = _fixture.Create<string>();
        var stageId = _fixture.Create<string>();
        var pipelineType = _fixture.Create<string>();

        var runInfo = _fixture.Create<PipelineRunInfo>();
        runInfo.Stages = new List<StageReport> { new() { Id = "prod" } };

        var pipelineRegistrations = _fixture.Build<PipelineRegistration>()
            .With(x => x.PartitionKey, PipelineRegistration.Prod)
            .With(x => x.StageId, "prod")
            .CreateMany(1)
            .ToList();

        var ruleReports = new List<RuleCompliancyReport>
        {
            new ()
            {
                HasDeviation = false,
                IsCompliant = false,
                RuleDescription = _fixture.Create<string>()
            }
        };

        _pipelineBreakerServiceMock
            .Setup<System.Threading.Tasks.Task<PipelineBreakerRegistrationReport>>(m => m.GetPreviousRegistrationResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerRegistrationReport { Result = PipelineBreakerResult.None });
        _pipelineBreakerServiceMock
            .Setup(m => m.EnrichPipelineInfoAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(runInfo);
        _registrationRepositoryMock
            .Setup(m => m.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(pipelineRegistrations);
        _pipelineBreakerServiceMock
            .Setup(m => m.GetPreviousComplianceResultAsync(It.IsAny<PipelineRunInfo>()))
            .ReturnsAsync(new PipelineBreakerReport { Result = PipelineBreakerResult.None });
        _pipelineBreakerServiceMock
            .Setup(m => m.GetCompliancy(It.IsAny<PipelineRunInfo>(), It.IsAny<IList<PipelineRegistration>>()))
            .ReturnsAsync(ruleReports);

        // Act
        var function = new PipelineBreakerFunction(_configWithBlocking, _validateInputServiceMock.Object,
            _pipelineBreakerServiceMock.Object, _registrationRepositoryMock.Object, _exclusionRepositoryMock.Object,
            _loggingServiceMock.Object);

        var result = await function.RunAsync(request, organization, projectId, runId, stageId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(OkObjectResult));
        var message = ((ObjectResult)result).Value!.ToString();
        message.ShouldStartWith(DecoratorResultMessages.NotCompliant);
    }

    #endregion [compliancy]
}
#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online;

public class ItemRescanFunction
{
    private const string _runAsyncRoute = "scan/{organization}/{projectId}/{ruleName}/{itemId}/{foreignProjectId?}";
    private const string _validationErrorsOccurred = "Errors validating the request.";
    private const string _unexpectedRuleNameError = "Unexpected rule name";
    private const string _invalidGuidProjectId = "Project ItemId is not a valid Guid";
    private const string _invalidGuidGitRepoId = "GitRepo ItemId is not a valid Guid";
    private const string _invalidIntPipelineId = "Pipeline ItemId is not a valid Int";
    private readonly IBuildPipelineRuleRescanProcess _buildPipelineRuleRescanProcess;
    private readonly IClassicReleasePipelineRuleRescanProcess _classicReleasePipelineRuleRescanProcess;
    private readonly IProjectRuleRescanProcess _projectRuleRescanProcess;
    private readonly IRepositoryRuleRescanProcess _repositoryRuleRescanProcess;
    private readonly ILoggingService _loggingService;

    public ItemRescanFunction(
        ILoggingService loggingService,
        IProjectRuleRescanProcess projectRuleRescanProcess,
        IRepositoryRuleRescanProcess repositoryRuleRescanProcess,
        IClassicReleasePipelineRuleRescanProcess classicReleasePipelineRuleRescanProcess,
        IBuildPipelineRuleRescanProcess buildPipelineRuleRescanProcess)
    {
        _loggingService = loggingService;
        _projectRuleRescanProcess = projectRuleRescanProcess;
        _repositoryRuleRescanProcess = repositoryRuleRescanProcess;
        _classicReleasePipelineRuleRescanProcess = classicReleasePipelineRuleRescanProcess;
        _buildPipelineRuleRescanProcess = buildPipelineRuleRescanProcess;
    }

    /// <summary>
    ///     Generic entrance for rescanning items. Will call the relevant Function based on the ruleName.
    /// </summary>
    [FunctionName(nameof(ItemRescanFunction))]
    public async Task<IActionResult> ItemRescan([HttpTrigger(AuthorizationLevel.Anonymous,
            Route = _runAsyncRoute)]
        HttpRequestMessage request, string? organization, Guid projectId, string? ruleName,
        string? itemId, Guid? foreignProjectId, CancellationToken cancellationToken)
    {
        try
        {
            ValidateRequest(organization, ruleName, itemId, projectId);

            if (itemId == ItemTypes.Dummy)
            {
                return new OkResult();
            }

            await RescanPerRuleType(ruleName, organization, projectId, itemId, foreignProjectId ?? projectId,
                cancellationToken);

            return new OkResult();
        }
        catch (ArgumentException ex)
        {
            var exceptionBaseMetaInformation = await LogExceptionAsync(ex);
            return new BadRequestObjectResult(
                $"{ex.Message} (CorrelationId:{exceptionBaseMetaInformation.CorrelationId})");
        }
        catch (AggregateException ex) when (ex.InnerExceptions.Any(exception => exception is ArgumentException))
        {
            var exceptionBaseMetaInformation = await LogExceptionAsync(ex);
            return new BadRequestObjectResult(
                $"{ex.Message} (CorrelationId:{exceptionBaseMetaInformation.CorrelationId})");
        }
        catch (Exception ex)
        {
            await LogExceptionAsync(ex);
            throw;
        }

        async Task<ExceptionBaseMetaInformation> LogExceptionAsync(Exception ex)
        {
            var exceptionBaseMetaInformation =
                request.ToExceptionBaseMetaInformation(organization, projectId, nameof(ItemRescanFunction));
            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionBaseMetaInformation, itemId, ruleName, ex);
            return exceptionBaseMetaInformation;
        }
    }

    #region Rescan Project Rules

    /// <summary>
    ///     Rescans the NobodyCanDeleteTheProject for the provided project (scannableProjectId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId and Guid scannableProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanNobodyCanDeleteTheProject))]
    public Task<IActionResult> RescanNobodyCanDeleteTheProject(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        ProjectRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.NobodyCanDeleteTheProject;
            await _projectRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    #endregion

    #region Rescan Repository Rules

    /// <summary>
    ///     Rescans the NobodyCanDeleteTheRepository for the provided gitRepo (scannableGitRepoId)
    /// </summary>
    /// <param name="request">
    ///     The request  object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid scannableRepositoryId and Guid itemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanNobodyCanDeleteTheRepository))]
    public Task<IActionResult> RescanNobodyCanDeleteTheRepository(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        RepositoryRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.NobodyCanDeleteTheRepository;
            await _repositoryRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    #endregion

    #region Rescan Build Rules

    /// <summary>
    ///     Rescans the NobodyCanDeleteBuilds for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanNobodyCanDeleteBuilds))]
    public Task<IActionResult> RescanNobodyCanDeleteBuilds(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.NobodyCanDeleteBuilds;
            await _buildPipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    /// <summary>
    ///     Rescans the BuildArtifactIsStoredSecure for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanBuildArtifactIsStoredSecure))]
    public Task<IActionResult> RescanBuildArtifactIsStoredSecure(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.BuildArtifactIsStoredSecure;
            await _buildPipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    /// <summary>
    ///     Rescans the BuildPipelineHasSonarqubeTask for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanBuildPipelineHasSonarqubeTask))]
    public Task<IActionResult> RescanBuildPipelineHasSonarqubeTask(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.BuildPipelineHasSonarqubeTask;
            await _buildPipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    /// <summary>
    ///     Rescans the BuildPipelineHasFortifyTask for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanBuildPipelineHasFortifyTask))]
    public Task<IActionResult> RescanBuildPipelineHasFortifyTask(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.BuildPipelineHasFortifyTask;
            await _buildPipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    /// <summary>
    ///     Rescans the BuildPipelineHasNexusIqTask for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanBuildPipelineHasNexusIqTask))]
    public Task<IActionResult> RescanBuildPipelineHasNexusIqTask(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.BuildPipelineHasNexusIqTask;
            await _buildPipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    /// <summary>
    ///     Rescans the BuildPipelineHasCredScanTask for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanBuildPipelineHasCredScanTask))]
    public Task<IActionResult> RescanBuildPipelineHasCredScanTask(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.BuildPipelineHasCredScanTask;
            await _buildPipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    /// <summary>
    ///     Rescans the BuildPipelineFollowsMainframeCobolProcess for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanBuildPipelineFollowsMainframeCobolProcess))]
    public Task<IActionResult> RescanBuildPipelineFollowsMainframeCobolProcess(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.BuildPipelineFollowsMainframeCobolProcess;
            await _buildPipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    #endregion

    #region Rescan Classic Release Rules

    /// <summary>
    ///     Rescans the NobodyCanDeleteReleases for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanNobodyCanDeleteReleases))]
    public Task<IActionResult> RescanNobodyCanDeleteReleases(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.NobodyCanDeleteReleases;
            await _classicReleasePipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    /// <summary>
    ///     Rescans the NobodyCanManagePipelineGatesAndDeploy for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanNobodyCanManagePipelineGatesAndDeploy))]
    public Task<IActionResult> RescanNobodyCanManagePipelineGatesAndDeploy(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.NobodyCanManagePipelineGatesAndDeploy;
            await _classicReleasePipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    /// <summary>
    ///     Rescans the ClassicReleasePipelineHasRequiredRetentionPolicy for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanClassicReleasePipelineHasRequiredRetentionPolicy))]
    public Task<IActionResult> RescanClassicReleasePipelineHasRequiredRetentionPolicy(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.ClassicReleasePipelineHasRequiredRetentionPolicy;
            await _classicReleasePipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    /// <summary>
    ///     Rescans the ClassicReleasePipelineUsesBuildArtifact for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanClassicReleasePipelineUsesBuildArtifact))]
    public Task<IActionResult> RescanClassicReleasePipelineUsesBuildArtifact(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.ClassicReleasePipelineUsesBuildArtifact;
            await _classicReleasePipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    /// <summary>
    ///     Rescans the ClassicReleasePipelineHasSm9ChangeTask for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanClassicReleasePipelineHasSm9ChangeTask))]
    public Task<IActionResult> RescanClassicReleasePipelineHasSm9ChangeTask(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.ClassicReleasePipelineHasSm9ChangeTask;
            await _classicReleasePipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    /// <summary>
    ///     Rescans the ClassicReleasePipelineIsBlockedWithout4EyesApproval for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanClassicReleasePipelineIsBlockedWithout4EyesApproval))]
    public Task<IActionResult> RescanClassicReleasePipelineIsBlockedWithout4EyesApproval(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.ClassicReleasePipelineIsBlockedWithout4EyesApproval;
            await _classicReleasePipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    /// <summary>
    ///     Rescans the ClassicReleasePipelineFollowsMainframeCobolReleaseProcess for the provided pipeline
    ///     (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanClassicReleasePipelineFollowsMainframeCobolReleaseProcess))]
    public Task<IActionResult> RescanClassicReleasePipelineFollowsMainframeCobolReleaseProcess(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.ClassicReleasePipelineFollowsMainframeCobolReleaseProcess;
            await _classicReleasePipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    #endregion

    #region Rescan Yaml Release Rules

    /// <summary>
    ///     Rescans the YamlReleasePipelineIsBlockedWithout4EyesApproval for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanYamlReleasePipelineIsBlockedWithout4EyesApproval))]
    public Task<IActionResult> RescanYamlReleasePipelineIsBlockedWithout4EyesApproval(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.YamlReleasePipelineIsBlockedWithout4EyesApproval;
            await _buildPipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    /// <summary>
    ///     Rescans the YamlReleasePipelineHasRequiredRetentionPolicy for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanYamlReleasePipelineHasRequiredRetentionPolicy))]
    public Task<IActionResult> RescanYamlReleasePipelineHasRequiredRetentionPolicy(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.YamlReleasePipelineHasRequiredRetentionPolicy;
            await _buildPipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    /// <summary>
    ///     Rescans the YamlReleasePipelineHasSm9ChangeTask for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanYamlReleasePipelineHasSm9ChangeTask))]
    public Task<IActionResult> RescanYamlReleasePipelineHasSm9ChangeTask(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.YamlReleasePipelineHasSm9ChangeTask;
            await _buildPipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    /// <summary>
    ///     Rescans the NobodyCanManageEnvironmentGatesAndDeploy for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanNobodyCanManageEnvironmentGatesAndDeploy))]
    public Task<IActionResult> RescanNobodyCanManageEnvironmentGatesAndDeploy(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.NobodyCanManageEnvironmentGatesAndDeploy;
            await _buildPipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    /// <summary>
    ///     Rescans the YamlReleasePipelineFollowsMainframeCobolReleaseProcess for the provided pipeline (ScannablePipelineId)
    /// </summary>
    /// <param name="request">
    ///     The request object is read from the body of the POST request and should consist of string
    ///     organization, Guid reportProjectId, Guid ScannablePipelineId and Guid ItemProjectId
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(RescanYamlReleasePipelineFollowsMainframeCobolReleaseProcess))]
    public Task<IActionResult> RescanYamlReleasePipelineFollowsMainframeCobolReleaseProcess(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        PipelineRuleRescanRequest request, CancellationToken cancellationToken) =>
        ProcessInternalAsync(async () =>
        {
            request.RuleName = RuleNames.YamlReleasePipelineFollowsMainframeCobolReleaseProcess;
            await _buildPipelineRuleRescanProcess.RescanAndUpdateReportAsync(request, cancellationToken);
            return new OkResult();
        });

    #endregion

    private async Task<IActionResult> RescanPerRuleType(string ruleName, string organization, Guid reportProjectId,
        string itemId, Guid itemProjectId, CancellationToken cancellationToken = default)
    {
        return ruleName switch
        {
            RuleNames.NobodyCanDeleteTheProject => await RescanNobodyCanDeleteTheProject(
                CreateProjectRuleRescanRequest(organization, reportProjectId, itemId), cancellationToken),
            RuleNames.NobodyCanDeleteTheRepository => await RescanNobodyCanDeleteTheRepository(
                CreateRepositoryRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                cancellationToken),
            RuleNames.NobodyCanDeleteBuilds => await RescanNobodyCanDeleteBuilds(
                CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                cancellationToken),
            RuleNames.BuildArtifactIsStoredSecure => await RescanBuildArtifactIsStoredSecure(
                CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                cancellationToken),
            RuleNames.BuildPipelineHasSonarqubeTask => await RescanBuildPipelineHasSonarqubeTask(
                CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                cancellationToken),
            RuleNames.BuildPipelineHasFortifyTask => await RescanBuildPipelineHasFortifyTask(
                CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                cancellationToken),
            RuleNames.BuildPipelineHasNexusIqTask => await RescanBuildPipelineHasNexusIqTask(
                CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                cancellationToken),
            RuleNames.BuildPipelineHasCredScanTask => await RescanBuildPipelineHasCredScanTask(
                CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                cancellationToken),
            RuleNames.BuildPipelineFollowsMainframeCobolProcess => await
                RescanBuildPipelineFollowsMainframeCobolProcess(
                    CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                    cancellationToken),
            RuleNames.NobodyCanDeleteReleases => await RescanNobodyCanDeleteReleases(
                CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                cancellationToken),
            RuleNames.NobodyCanManagePipelineGatesAndDeploy => await RescanNobodyCanManagePipelineGatesAndDeploy(
                CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                cancellationToken),
            RuleNames.ClassicReleasePipelineHasRequiredRetentionPolicy => await
                RescanClassicReleasePipelineHasRequiredRetentionPolicy(
                    CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                    cancellationToken),
            RuleNames.ClassicReleasePipelineUsesBuildArtifact => await RescanClassicReleasePipelineUsesBuildArtifact(
                CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                cancellationToken),
            RuleNames.ClassicReleasePipelineHasSm9ChangeTask => await RescanClassicReleasePipelineHasSm9ChangeTask(
                CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                cancellationToken),
            RuleNames.ClassicReleasePipelineIsBlockedWithout4EyesApproval => await
                RescanClassicReleasePipelineIsBlockedWithout4EyesApproval(
                    CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                    cancellationToken),
            RuleNames.ClassicReleasePipelineFollowsMainframeCobolReleaseProcess => await
                RescanClassicReleasePipelineFollowsMainframeCobolReleaseProcess(
                    CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                    cancellationToken),
            RuleNames.YamlReleasePipelineFollowsMainframeCobolReleaseProcess => await
                RescanYamlReleasePipelineFollowsMainframeCobolReleaseProcess(
                    CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                    cancellationToken),
            RuleNames.YamlReleasePipelineIsBlockedWithout4EyesApproval => await
                RescanYamlReleasePipelineIsBlockedWithout4EyesApproval(
                    CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                    cancellationToken),
            RuleNames.YamlReleasePipelineHasRequiredRetentionPolicy => await
                RescanYamlReleasePipelineHasRequiredRetentionPolicy(
                    CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                    cancellationToken),
            RuleNames.YamlReleasePipelineHasSm9ChangeTask => await RescanYamlReleasePipelineHasSm9ChangeTask(
                CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                cancellationToken),
            RuleNames.NobodyCanManageEnvironmentGatesAndDeploy => await RescanNobodyCanManageEnvironmentGatesAndDeploy(
                CreatePipelineRuleRescanRequest(organization, reportProjectId, itemId, itemProjectId),
                cancellationToken),
            _ => throw new ArgumentException(_unexpectedRuleNameError, nameof(ruleName))
        };
    }

    private static ProjectRuleRescanRequest CreateProjectRuleRescanRequest(string organization, Guid reportProjectId,
        string itemId) =>
        new(organization, reportProjectId, StringToGuidOrThrow(itemId, _invalidGuidProjectId, nameof(itemId)), null);

    private static RepositoryRuleRescanRequest CreateRepositoryRuleRescanRequest(string organization,
        Guid reportProjectId, string itemId, Guid itemProjectId) =>
        new(organization, reportProjectId, StringToGuidOrThrow(itemId, _invalidGuidGitRepoId, nameof(itemId)),
            itemProjectId, null);

    private static PipelineRuleRescanRequest CreatePipelineRuleRescanRequest(string organization, Guid reportProjectId,
        string itemId, Guid itemProjectId) =>
        new(organization, reportProjectId, StringToIntOrThrow(itemId, _invalidIntPipelineId, nameof(itemId)),
            itemProjectId, null);

    private static void ValidateRequest([NotNull] string? organization, [NotNull] string? ruleName,
        [NotNull] string? itemId, Guid projectId)
    {
        var exceptions = new List<Exception>();
        if (string.IsNullOrEmpty(organization))
        {
            exceptions.Add(new ArgumentNullException(nameof(organization)));
        }

        if (string.IsNullOrEmpty(itemId))
        {
            exceptions.Add(new ArgumentNullException(nameof(itemId)));
        }

        if (string.IsNullOrEmpty(ruleName))
        {
            exceptions.Add(new ArgumentNullException(nameof(ruleName)));
        }

        if (projectId == Guid.Empty)
        {
            exceptions.Add(new ArgumentNullException(nameof(projectId)));
        }

        if (exceptions.Any())
        {
            throw new AggregateException(_validationErrorsOccurred, exceptions);
        }

        // Warning can be disabled since an exception will be thrown
        // if any of the parameters is null.
#pragma warning disable CS8777
    }
#pragma warning restore CS8777

    private static Guid StringToGuidOrThrow(string input, string parameterName, string errorMessage)
    {
        if (Guid.TryParse(input, out var parsedGuid))
        {
            return parsedGuid;
        }

        throw new ArgumentException(errorMessage, parameterName);
    }

    private static int StringToIntOrThrow(string input, string parameterName, string errorMessage)
    {
        if (int.TryParse(input, out var parsedInt))
        {
            return parsedInt;
        }

        throw new ArgumentException(errorMessage, parameterName);
    }

    private async Task<IActionResult> ProcessInternalAsync(Func<Task<IActionResult>> func)
    {
        try
        {
            return await func();
        }
        catch (Exception e)
        {
            var exceptionReport = new ExceptionReport(e);
            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionReport);
            throw;
        }
    }
}
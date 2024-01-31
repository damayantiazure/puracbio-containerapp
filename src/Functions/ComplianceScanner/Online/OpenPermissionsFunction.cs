#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Interfaces.OpenPermissions;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Requests.OpenPermissions;
using Rabobank.Compliancy.Application.Security;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;
using Rabobank.Compliancy.Infrastructure.AzureDevOps;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online;

public class OpenPermissionsFunction : BaseFunction
{
    private readonly ICheckAuthorizationProcess _checkAuthorizationProcess;

    private readonly IOpenPipelinePermissionsProcess<AzdoBuildDefinitionPipeline>
        _openBuildDefinitionPipelinePermissionProcess;

    private readonly IOpenGitRepoPermissionsProcess _openGitRepoPermissionProcess;
    private readonly IOpenPipelinePermissionsProcess<AzdoReleaseDefinitionPipeline> _openReleasePermissionProcess;
    private readonly ILoggingService _loggingService;

    public OpenPermissionsFunction(IHttpContextAccessor httpContextAccessor,
        ILoggingService loggingService,
        ICheckAuthorizationProcess checkAuthorizationProcess,
        IOpenGitRepoPermissionsProcess openRepositoryPermissionProcess,
        IOpenPipelinePermissionsProcess<AzdoBuildDefinitionPipeline> openBuildDefinitionPipelinePermissionProcess,
        IOpenPipelinePermissionsProcess<AzdoReleaseDefinitionPipeline> openReleasePermissionProcess,
        ISecurityContext securityContext)
        : base(httpContextAccessor, loggingService, securityContext)
    {
        _loggingService = loggingService;
        _checkAuthorizationProcess = checkAuthorizationProcess;
        _openGitRepoPermissionProcess = openRepositoryPermissionProcess;
        _openBuildDefinitionPipelinePermissionProcess = openBuildDefinitionPipelinePermissionProcess;
        _openReleasePermissionProcess = openReleasePermissionProcess;
    }

    /// <summary>
    ///     OpenRepositoryPermissionsAsync will open permission for a repository, pipeline or a project.
    /// </summary>
    /// <param name="openRepositoryPermissionsRequest">
    ///     The request object is read from the body of the POST request and
    ///     converted to <see cref="OpenGitRepoPermissionsRequest" />.
    /// </param>
    /// <param name="httpRequest">Represents the incoming side of an individual HTTP request.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the action as <see cref="IActionResult" />.</returns>
    [FunctionName(nameof(OpenRepositoryPermissionsAsync))]
    public async Task<IActionResult?> OpenRepositoryPermissionsAsync([HttpTrigger(AuthorizationLevel.Anonymous,
            Route = "open-permissions/{organization}/{projectId}/Repository/{gitRepoId}")]
        OpenGitRepoPermissionsRequest openRepositoryPermissionsRequest, HttpRequest httpRequest,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isAuthorized = await IsAuthorized(openRepositoryPermissionsRequest, httpRequest, cancellationToken);

            if (!isAuthorized)
            {
                return new UnauthorizedResult();
            }

            await _openGitRepoPermissionProcess.OpenPermissionAsync(openRepositoryPermissionsRequest,
                cancellationToken);

            return new OkResult();
        }
        catch (Exception ex) when (
            ex is IsProductionItemException or SourceItemNotFoundException)
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
            var exceptionBaseMetaInformation = httpRequest.ToExceptionBaseMetaInformation(
                openRepositoryPermissionsRequest.Organization,
                openRepositoryPermissionsRequest.ProjectId, nameof(OpenRepositoryPermissionsAsync));

            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionBaseMetaInformation, ex);

            return exceptionBaseMetaInformation;
        }
    }

    /// <summary>
    ///     OpenBuildPermissionsAsync will open permission for a repository, pipeline or a project.
    /// </summary>
    /// <param name="openPipelinePermissionsRequest">
    ///     The request object is read from the body of the POST request and converted
    ///     to <see cref="OpenPipelinePermissionsRequest" />.
    /// </param>
    /// <param name="httpRequest">Represents the incoming side of an individual HTTP request.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the action as <see cref="IActionResult" />.</returns>
    [FunctionName(nameof(OpenBuildPermissionsAsync))]
    public async Task<IActionResult?> OpenBuildPermissionsAsync([HttpTrigger(AuthorizationLevel.Anonymous,
            Route = "open-permissions/{organization}/{projectId}/Build/{pipelineId}")]
        OpenPipelinePermissionsRequest<AzdoBuildDefinitionPipeline> openPipelinePermissionsRequest,
        HttpRequest httpRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var isAuthorized = await IsAuthorized(openPipelinePermissionsRequest, httpRequest, cancellationToken);
            if (!isAuthorized)
            {
                return new UnauthorizedResult();
            }

            await _openBuildDefinitionPipelinePermissionProcess.OpenPermissionAsync(openPipelinePermissionsRequest,
                cancellationToken);

            return new OkResult();
        }
        catch (Exception ex) when (
            ex is IsProductionItemException or SourceItemNotFoundException)
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
            var exceptionBaseMetaInformation = httpRequest.ToExceptionBaseMetaInformation(
                openPipelinePermissionsRequest.Organization,
                openPipelinePermissionsRequest.ProjectId, nameof(OpenBuildPermissionsAsync));

            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionBaseMetaInformation, ex);

            return exceptionBaseMetaInformation;
        }
    }

    /// <summary>
    ///     OpenReleasePermissionsAsync will open permission for a repository, pipeline or a project.
    /// </summary>
    /// <param name="openPipelinePermissionsRequest">
    ///     The request object is read from the body of the POST request and converted
    ///     to <see cref="OpenPipelinePermissionsRequest" />.
    /// </param>
    /// <param name="httpRequest">Represents the incoming side of an individual HTTP request.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the action as <see cref="IActionResult" />.</returns>
    [FunctionName(nameof(OpenReleasePermissionsAsync))]
    public async Task<IActionResult?> OpenReleasePermissionsAsync([HttpTrigger(AuthorizationLevel.Anonymous,
            Route = "open-permissions/{organization}/{projectId}/Release/{pipelineId}")]
        OpenPipelinePermissionsRequest<AzdoReleaseDefinitionPipeline> openPipelinePermissionsRequest,
        HttpRequest httpRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var isAuthorized = await IsAuthorized(openPipelinePermissionsRequest, httpRequest, cancellationToken);
            if (!isAuthorized)
            {
                return new UnauthorizedResult();
            }

            await _openReleasePermissionProcess.OpenPermissionAsync(openPipelinePermissionsRequest, cancellationToken);

            return new OkResult();
        }
        catch (Exception ex) when (
            ex is IsProductionItemException or SourceItemNotFoundException)
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
            var exceptionBaseMetaInformation = httpRequest.ToExceptionBaseMetaInformation(
                openPipelinePermissionsRequest.Organization,
                openPipelinePermissionsRequest.ProjectId, nameof(OpenReleasePermissionsAsync));

            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionBaseMetaInformation, ex);

            return exceptionBaseMetaInformation;
        }
    }

    private async Task<bool> IsAuthorized<T>(OpenPermissionsRequestBase<T> openPipelinePermissionsRequest,
        HttpRequest httpRequest,
        CancellationToken cancellationToken)
        where T : IProtectedResource
    {
        var authorizationHeader = httpRequest.GetAuthorizationTokenOrDefault();
        var authorizationRequest = new AuthorizationRequest(openPipelinePermissionsRequest.ProjectId,
            openPipelinePermissionsRequest.Organization);
        var isAuthorized =
            await _checkAuthorizationProcess.IsAuthorized(authorizationRequest, authorizationHeader,
                cancellationToken);
        return isAuthorized;
    }
}
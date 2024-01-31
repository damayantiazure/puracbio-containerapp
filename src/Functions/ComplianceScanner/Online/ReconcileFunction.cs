#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Interfaces.Reconcile;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Security;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Rules.Exceptions;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online;

public class ReconcileFunction : BaseFunction
{
    private readonly IReconcileProcess _reconcileProcess;
    private readonly IItemReconcileProcess _itemReconcileProcess;
    private readonly ILoggingService _loggingService;
    private readonly IProjectReconcileProcess _projectReconcileProcess;
    private readonly ICheckAuthorizationProcess _checkAuthorizationProcess;

    public ReconcileFunction(
        IReconcileProcess reconcileProcess, IItemReconcileProcess itemReconcileProcess, IProjectReconcileProcess projectReconcileProcess,
        ILoggingService loggingService, ICheckAuthorizationProcess checkAuthorizationProcess,
        IHttpContextAccessor httpContextAccessor, ISecurityContext securityContext)
        : base(httpContextAccessor, loggingService, securityContext)
    {
        _reconcileProcess = reconcileProcess;
        _itemReconcileProcess = itemReconcileProcess;
        _projectReconcileProcess = projectReconcileProcess;
        _loggingService = loggingService;
        _checkAuthorizationProcess = checkAuthorizationProcess;
    }

    /// <summary>
    /// Run async will reconcile a project or an item rule.
    /// </summary>
    /// <param name="reconcileRequest">The request object is read from the body of the POST request and converted to <see cref="ReconcileRequest"/>.</param>
    /// <param name="httpRequest">Represents the incoming side of an individual HTTP request.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the action as <see cref="IActionResult"/>.</returns>
    [FunctionName(nameof(ReconcileFunction))]
    public Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous,
            Route = "reconcile/{organization}/{projectId}/{ruleName}/{itemId}")]
        ReconcileRequest reconcileRequest, HttpRequest httpRequest, CancellationToken cancellationToken = default) =>
        PerformProcessHandlerAsync(httpRequest, reconcileRequest, _reconcileProcess, nameof(ReconcileFunction),
            cancellationToken);

    /// <summary>
    /// The ProjectReconcileAsync will reconcile a specific rule for a project.
    /// </summary>
    /// <param name="reconcileRequest">The request object is read from the body of the POST request and converted to <see cref="ReconcileRequest"/>.</param>
    /// <param name="httpRequest">Represents the incoming side of an individual HTTP request.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the action as <see cref="IActionResult"/>.</returns>
    [FunctionName(nameof(ProjectReconcileAsync))]
    public Task<IActionResult> ProjectReconcileAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        ReconcileRequest reconcileRequest, HttpRequest httpRequest, CancellationToken cancellationToken = default) =>
        PerformProcessHandlerAsync(httpRequest, reconcileRequest, _projectReconcileProcess,
            nameof(ProjectReconcileAsync), cancellationToken);

    /// <summary>
    /// The ItemReconcileAsync will reconcile a specific rule for a repository or a pipeline.
    /// </summary>
    /// <param name="reconcileRequest">The request object is read from the body of the POST request and converted to <see cref="ReconcileRequest"/>.</param>
    /// <param name="httpRequest">Represents the incoming side of an individual HTTP request.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the action as <see cref="IActionResult"/>.</returns>
    [FunctionName(nameof(ItemReconcileAsync))]
    public Task<IActionResult> ItemReconcileAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post)]
        ReconcileRequest reconcileRequest, HttpRequest httpRequest, CancellationToken cancellationToken = default) =>
        PerformProcessHandlerAsync(httpRequest, reconcileRequest, _itemReconcileProcess, nameof(ItemReconcileAsync),
            cancellationToken);

    private async Task<IActionResult> PerformProcessHandlerAsync(HttpRequest httpRequest, ReconcileRequest reconcileRequest,
        IReconcileBase reconcileProcess, string functionName, CancellationToken cancellationToken = default)
    {
        try
        {
            CheckForExistingRuleName(reconcileProcess, reconcileRequest.RuleName);

            var authorizationHeader = httpRequest.GetAuthorizationTokenOrDefault();
            var authorizationRequest = new AuthorizationRequest(reconcileRequest.ProjectId, reconcileRequest.Organization);

            if (!await _checkAuthorizationProcess.IsAuthorized(authorizationRequest, authorizationHeader, cancellationToken))
            {
                return new UnauthorizedResult();
            }

            await reconcileProcess.ReconcileAsync(reconcileRequest, cancellationToken);

            return new OkResult();
        }
        catch (Exception ex) when (
            ex is InvalidClassicPipelineException
                or InvalidYamlPipelineException
                or EnvironmentNotFoundException
                or InvalidEnvironmentException
                or ArgumentException)
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
                httpRequest.ToExceptionBaseMetaInformation(reconcileRequest.Organization, reconcileRequest.ProjectId,
                    functionName);
            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionBaseMetaInformation, reconcileRequest.ItemId,
                reconcileRequest.RuleName, ex);
            return exceptionBaseMetaInformation;
        }
    }

    private static void CheckForExistingRuleName(IReconcileBase reconcileProcess, string ruleName)
    {
        if (!reconcileProcess.HasRuleName(ruleName))
        {
            throw new ArgumentOutOfRangeException(nameof(ruleName));
        }
    }
}
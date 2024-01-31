#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;
using Rabobank.Compliancy.Application.MonitoringDashboard.Dto;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.MonitoringDashboard;

public class MonitoringDashboardFunction
{
    private readonly IMonitoringDashboardTileProcess _monitoringDashboardTileProcess;

    public MonitoringDashboardFunction(IMonitoringDashboardTileProcess monitoringDashboardTileProcess)
    {
        _monitoringDashboardTileProcess = monitoringDashboardTileProcess;
    }

    [FunctionName(nameof(AuditLoggingError))]
    public async Task<IActionResult> AuditLoggingError([HttpTrigger(AuthorizationLevel.Function,
        Route = nameof(AuditLoggingError))] CancellationToken cancellationToken)
    {
        return await HandleRequest(new AuditLoggingErrorDashboardTileInformation(), cancellationToken);
    }

    [FunctionName(nameof(AuditLogging))]
    public async Task<IActionResult> AuditLogging([HttpTrigger(AuthorizationLevel.Function,
        Route = nameof(AuditLogging))] CancellationToken cancellationToken)
    {
        return await HandleRequest(new AuditLoggingDashboardTileInformation(), cancellationToken);
    }

    [FunctionName(nameof(AuditLoggingPoisonQueue))]
    public async Task<IActionResult> AuditLoggingPoisonQueue([HttpTrigger(AuthorizationLevel.Function,
        Route = nameof(AuditLoggingPoisonQueue))] CancellationToken cancellationToken)
    {
        return await HandleRequest(new AuditLoggingPoisonQueueDashboardTileInformation(), cancellationToken);
    }

    [FunctionName(nameof(ComplianceScannerItems))]
    public async Task<IActionResult> ComplianceScannerItems([HttpTrigger(AuthorizationLevel.Function,
        Route = nameof(ComplianceScannerItems))] CancellationToken cancellationToken)
    {
        return await HandleRequest(new ComplianceScannerItemsDashboardTileInformation(), cancellationToken);
    }

    [FunctionName(nameof(ComplianceScannerOnlineError))]
    public async Task<IActionResult> ComplianceScannerOnlineError([HttpTrigger(AuthorizationLevel.Function,
        Route = nameof(ComplianceScannerOnlineError))] CancellationToken cancellationToken)
    {
        return await HandleRequest(new ComplianceScannerOnlineErrorDashboardTileInformation(), cancellationToken);
    }

    [FunctionName(nameof(ComplianceScannerPrinciples))]
    public async Task<IActionResult> ComplianceScannerPrinciples([HttpTrigger(AuthorizationLevel.Function,
        Route = nameof(ComplianceScannerPrinciples))] CancellationToken cancellationToken)
    {
        return await HandleRequest(new ComplianceScannerPrinciplesDashboardTileInformation(), cancellationToken);
    }

    [FunctionName(nameof(ComplianceScannerRules))]
    public async Task<IActionResult> ComplianceScannerRules([HttpTrigger(AuthorizationLevel.Function,
        Route = nameof(ComplianceScannerRules))] CancellationToken cancellationToken)
    {
        return await HandleRequest(new ComplianceScannerRulesDashboardTileInformation(), cancellationToken);
    }

    [FunctionName(nameof(CompliancyScannerCis))]
    public async Task<IActionResult> CompliancyScannerCis([HttpTrigger(AuthorizationLevel.Function,
        Route = nameof(CompliancyScannerCis))] CancellationToken cancellationToken)
    {
        return await HandleRequest(new CompliancyScannerCisDashboardTileInformation(), cancellationToken);
    }

    [FunctionName(nameof(ErrorHandling))]
    public async Task<IActionResult> ErrorHandling([HttpTrigger(AuthorizationLevel.Function,
        Route = nameof(ErrorHandling))] CancellationToken cancellationToken)
    {
        return await HandleRequest(new ErrorHandlingDashboardTileInformation(), cancellationToken);
    }

    [FunctionName(nameof(HooksFailures))]
    public async Task<IActionResult> HooksFailures([HttpTrigger(AuthorizationLevel.Function,
        Route = nameof(HooksFailures))] CancellationToken cancellationToken)
    {
        return await HandleRequest(new HooksFailuresDashboardTileInformation(), cancellationToken);
    }

    [FunctionName(nameof(PipelineBreakerDecoratorError))]
    public async Task<IActionResult> PipelineBreakerDecoratorError([HttpTrigger(AuthorizationLevel.Function,
        Route = nameof(PipelineBreakerDecoratorError))] CancellationToken cancellationToken)
    {
        return await HandleRequest(new PipelineBreakerDecoratorErrorDashboardTileInformation(), cancellationToken);
    }

    [FunctionName(nameof(PipelineBreakerError))]
    public async Task<IActionResult> PipelineBreakerError([HttpTrigger(AuthorizationLevel.Function,
        Route = nameof(PipelineBreakerError))] CancellationToken cancellationToken)
    {
        return await HandleRequest(new PipelineBreakerErrorDashboardTileInformation(), cancellationToken);
    }

    [FunctionName(nameof(Sm9ChangesError))]
    public async Task<IActionResult> Sm9ChangesError([HttpTrigger(AuthorizationLevel.Function,
        Route = nameof(Sm9ChangesError))] CancellationToken cancellationToken)
    {
        return await HandleRequest(new Sm9ChangesErrorDashboardTileInformation(), cancellationToken);
    }

    [FunctionName(nameof(ValidateGatesError))]
    public async Task<IActionResult> ValidateGatesError([HttpTrigger(AuthorizationLevel.Function,
        Route = nameof(ValidateGatesError))] CancellationToken cancellationToken)
    {
        return await HandleRequest(new ValidateGatesErrorDashboardTileInformation(), cancellationToken);
    }

    private async Task<IActionResult> HandleRequest(IMonitoringDashboardTileProcessInformation monitoringDashboardTileProcessInformation, CancellationToken cancellationToken)
    {
        try
        {
            var content = await _monitoringDashboardTileProcess.GetWidgetContentForTile(monitoringDashboardTileProcessInformation, cancellationToken);

            return new ContentResult
            {
                Content = content,
                ContentType = "text/html"
            };
        }
        catch (InvalidOperationException)
        {
            return new UnauthorizedResult();
        }
    }
}
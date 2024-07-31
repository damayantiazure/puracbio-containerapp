using Rabobank.Compliancy.Application.Interfaces.Reconcile;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;

namespace Rabobank.Compliancy.Application.Reconcile;

/// <inheritdoc/>
public class ReconcileProcess : ReconcileProcessBase, IReconcileProcess
{
    private readonly IProjectReconcileProcess _projectReconcileProcess;
    private readonly IItemReconcileProcess _itemReconcileProcess;

    public ReconcileProcess(
        IProjectReconcileProcess projectReconcileProcess,
        IItemReconcileProcess itemReconcileProcess,
        IProjectService projectService,
        ICompliancyReportService compliancyReportService) : base(projectService, compliancyReportService)
    {
        _projectReconcileProcess = projectReconcileProcess ?? throw new ArgumentNullException(nameof(projectReconcileProcess));
        _itemReconcileProcess = itemReconcileProcess ?? throw new ArgumentNullException(nameof(itemReconcileProcess));
    }

    /// <inheritdoc/>
    public bool HasRuleName(string ruleName)
    {
        return _itemReconcileProcess.HasRuleName(ruleName) || _projectReconcileProcess.HasRuleName(ruleName);
    }

    /// <inheritdoc/>
    public async Task ReconcileAsync(ReconcileRequest reconcileRequest, CancellationToken cancellationToken)
    {
        await _projectReconcileProcess.ReconcileAsync(reconcileRequest, cancellationToken);
        await _itemReconcileProcess.ReconcileAsync(reconcileRequest, cancellationToken);
    }
}
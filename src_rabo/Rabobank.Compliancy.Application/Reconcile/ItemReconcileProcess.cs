using Rabobank.Compliancy.Application.Interfaces.Reconcile;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Core.Rules.Processors;

namespace Rabobank.Compliancy.Application.Reconcile;

/// <inheritdoc/>
public class ItemReconcileProcess : ReconcileProcessBase, IItemReconcileProcess
{
    private readonly IReconcileProcessor _reconcileProcessor;

    public ItemReconcileProcess(IReconcileProcessor reconcileProcessor,
        IProjectService projectService,
        ICompliancyReportService compliancyReportService) : base(projectService, compliancyReportService)
    {
        _reconcileProcessor = reconcileProcessor ?? throw new ArgumentNullException(nameof(reconcileProcessor));
    }

    /// <inheritdoc/>
    public bool HasRuleName(string ruleName)
    {
        return GetItemReconcile(ruleName) != null;
    }

    /// <inheritdoc/>
    public async Task ReconcileAsync(ReconcileRequest reconcileRequest, CancellationToken cancellationToken)
    {
        var itemReconcile = GetItemReconcile(reconcileRequest.RuleName);
        if (itemReconcile == null)
        {
            return;
        }

        var reevaluationResult = await itemReconcile.ReconcileAndEvaluateAsync(reconcileRequest.Organization, reconcileRequest.ProjectId.ToString(), reconcileRequest.ItemId);
        await UpdateReportAsync(reconcileRequest, reevaluationResult, cancellationToken);
    }

    private IReconcile GetItemReconcile(string ruleName)
    {
        var itemReconcile = _reconcileProcessor.GetAllItemReconcile();
        return itemReconcile.FirstOrDefault(x => x.Name.Equals(ruleName, StringComparison.OrdinalIgnoreCase));
    }
}
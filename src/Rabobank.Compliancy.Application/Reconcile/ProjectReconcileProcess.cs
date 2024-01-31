using Rabobank.Compliancy.Application.Interfaces.Reconcile;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Core.Rules.Processors;

namespace Rabobank.Compliancy.Application.Reconcile;

/// <inheritdoc/>
public class ProjectReconcileProcess : ReconcileProcessBase, IProjectReconcileProcess
{
    private readonly IReconcileProcessor _reconcileProcessor;
    protected override bool _hasConcernsProjectRule { get; } = true;
    public ProjectReconcileProcess(IReconcileProcessor reconcileProcessor,
        IProjectService projectService,
        ICompliancyReportService compliancyReportService) : base(projectService, compliancyReportService)
    {
        _reconcileProcessor = reconcileProcessor ?? throw new ArgumentNullException(nameof(reconcileProcessor));
    }

    /// <inheritdoc/>
    public bool HasRuleName(string ruleName)
    {
        return GetProjectReconcile(ruleName) != null;
    }

    /// <inheritdoc/>
    public async Task ReconcileAsync(ReconcileRequest reconcileRequest, CancellationToken cancellationToken)
    {
        var projectRule = GetProjectReconcile(reconcileRequest.RuleName);
        if (projectRule == null)
        {
            return;
        }

        var reevaluationResult = await projectRule.ReconcileAndEvaluateAsync(reconcileRequest.Organization, reconcileRequest.ProjectId.ToString());
        await UpdateReportAsync(reconcileRequest, reevaluationResult, cancellationToken);
    }

    private IProjectReconcile GetProjectReconcile(string ruleName)
    {
        var projectReconcileRules = _reconcileProcessor.GetAllProjectReconcile();
        return projectReconcileRules.FirstOrDefault(x => x.Name.Equals(ruleName, StringComparison.OrdinalIgnoreCase));
    }
}
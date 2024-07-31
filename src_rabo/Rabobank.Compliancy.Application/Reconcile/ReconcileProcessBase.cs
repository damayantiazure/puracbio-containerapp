using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;

namespace Rabobank.Compliancy.Application.Reconcile;

public class ReconcileProcessBase
{
    private readonly IProjectService _projectService;
    private readonly ICompliancyReportService _compliancyReportService;

    /// <summary>
    /// Updates the compliancy report project rule.
    /// </summary>
    protected virtual bool _hasConcernsProjectRule { get; } = false;

    public ReconcileProcessBase(IProjectService projectService, ICompliancyReportService compliancyReportService)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _compliancyReportService = compliancyReportService ?? throw new ArgumentNullException(nameof(compliancyReportService));
    }

    /// <summary>
    /// Performs an update to the compliancy report based on the provided information.
    /// </summary>
    /// <param name="reconcileRequest">The reconcile request.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Represents an asynchronous operation as <see cref="Task"/>.</returns>
    protected async Task UpdateReportAsync(ReconcileRequest reconcileRequest, bool result, CancellationToken cancellationToken = default)
    {
        var project = await _projectService.GetProjectByIdAsync(reconcileRequest.Organization, reconcileRequest.ProjectId, cancellationToken);
        await _compliancyReportService.UpdateComplianceStatusAsync(project, reconcileRequest.ProjectId, reconcileRequest.ItemId, reconcileRequest.RuleName, _hasConcernsProjectRule, result);
    }
}
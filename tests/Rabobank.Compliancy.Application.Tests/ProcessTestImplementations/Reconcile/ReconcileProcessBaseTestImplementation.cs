using Rabobank.Compliancy.Application.Reconcile;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;

namespace Rabobank.Compliancy.Application.Tests.ProcessTestImplementations.Reconcile;

public class ReconcileProcessBaseTestImplementation : ReconcileProcessBase
{
    public ReconcileProcessBaseTestImplementation(IProjectService projectService, ICompliancyReportService compliancyReportService)
        : base(projectService, compliancyReportService)
    {
    }

    public async Task UsesUpdateReportAsync(ReconcileRequest reconcileRequest, bool result, CancellationToken cancellationToken = default)
    {
        await UpdateReportAsync(reconcileRequest, result, cancellationToken);
    }
}
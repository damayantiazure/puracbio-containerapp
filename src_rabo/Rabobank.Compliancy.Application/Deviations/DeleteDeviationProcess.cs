using Rabobank.Compliancy.Application.Interfaces.Deviations;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;

namespace Rabobank.Compliancy.Application.Deviations;

/// <inheritdoc/>
public class DeleteDeviationProcess : IDeleteDeviationProcess
{
    private readonly IProjectService _projectService;
    private readonly IDeviationService _deviationService;
    private readonly ICompliancyReportService _compliancyReportService;

    public DeleteDeviationProcess(IProjectService projectService, IDeviationService deviationService,
        ICompliancyReportService compliancyReportService)
    {
        _projectService = projectService;
        _deviationService = deviationService;
        _compliancyReportService = compliancyReportService;
    }

    /// <inheritdoc/>
    public async Task DeleteDeviationAsync(DeleteDeviationRequest request, CancellationToken cancellationToken = default)
    {
        var project = await _projectService.GetProjectByIdAsync(request.Organization, request.ProjectId, cancellationToken);

        var deviation = await _deviationService.GetDeviationAsync(project, request.RuleName, request.ItemId
            , request.CiIdentifier, request.ForeignProjectId, cancellationToken);

        if (deviation == null)
        {
            return;
        }

        // delete the deviation from the table storage
        await _deviationService.DeleteDeviationAsync(deviation, cancellationToken);

        // remove the deviation from the compliancy report
        await _compliancyReportService.RemoveDeviationFromReportAsync(deviation);

        // Send deviation delete record to the queue
        await _deviationService.SendDeviationUpdateRecord(deviation, Domain.Compliancy.Deviations.DeviationReportLogRecordType.Delete);
    }
}
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Rules.Processors;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.ItemRescan;

public abstract class ItemRescanProcessBase
{
    protected readonly IRuleProcessor _ruleProcessor;
    protected readonly IProjectService _projectService;
    private readonly ICompliancyReportService _reportService;

    protected ItemRescanProcessBase(IRuleProcessor ruleProcessor, IProjectService projectService, ICompliancyReportService reportService)
    {
        _ruleProcessor = ruleProcessor ?? throw new ArgumentNullException(nameof(ruleProcessor));
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
    }

    protected async Task<Project> GetParentProject(string organization, Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _projectService.GetProjectByIdAsync(organization, projectId, cancellationToken);
    }

    protected async Task UpdateReportAsync(RuleRescanRequestBase request, bool evaluationResult, CancellationToken cancellationToken = default)
    {
        var project = await _projectService.GetProjectByIdAsync(request.Organization, request.ReportProjectId, cancellationToken);
        await _reportService.UpdateComplianceStatusAsync(project, request.ItemProjectId, request.ItemIdAsString, request.RuleName, request.ConcernsProjectRule, evaluationResult);
    }
}
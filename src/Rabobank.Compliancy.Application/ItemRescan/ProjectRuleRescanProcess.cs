using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Core.Rules.Processors;

namespace Rabobank.Compliancy.Application.ItemRescan;

public class ProjectRuleRescanProcess : ItemRescanProcessBase, IProjectRuleRescanProcess
{
    public ProjectRuleRescanProcess(IRuleProcessor ruleProcessor, IProjectService projectService, ICompliancyReportService reportService)
        : base(ruleProcessor, projectService, reportService)
    {
    }

    public async Task RescanAndUpdateReportAsync(ProjectRuleRescanRequest request, CancellationToken cancellationToken = default)
    {
        var rule = _ruleProcessor.GetRuleByName<IProjectRule>(request.RuleName);
        var itemProject = await _projectService.GetProjectByIdAsync(request.Organization, request.ItemProjectId, cancellationToken);

        var result = await rule.EvaluateAsync(itemProject);

        await UpdateReportAsync(request, result, cancellationToken);
    }
}
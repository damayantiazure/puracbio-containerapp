using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Core.Rules.Processors;

namespace Rabobank.Compliancy.Application.ItemRescan;

public class RepositoryRuleRescanProcess : ItemRescanProcessBase, IRepositoryRuleRescanProcess
{
    private readonly IGitRepoService _gitRepoService;

    public RepositoryRuleRescanProcess(IRuleProcessor ruleProcessor, IProjectService projectService, ICompliancyReportService reportService, IGitRepoService gitRepoService)
        : base(ruleProcessor, projectService, reportService)
    {
        _gitRepoService = gitRepoService ?? throw new ArgumentNullException(nameof(gitRepoService));
    }

    public async Task RescanAndUpdateReportAsync(RepositoryRuleRescanRequest request, CancellationToken cancellationToken = default)
    {
        var rule = _ruleProcessor.GetRuleByName<IRepositoryRule>(request.RuleName);
        var itemProject = await GetParentProject(request.Organization, request.ItemProjectId, cancellationToken);
        var gitRepo = await _gitRepoService.GetGitRepoByIdAsync(itemProject, request.GitRepoId, cancellationToken);
        var result = await rule.EvaluateAsync(gitRepo);
        await UpdateReportAsync(request, result, cancellationToken);
    }
}
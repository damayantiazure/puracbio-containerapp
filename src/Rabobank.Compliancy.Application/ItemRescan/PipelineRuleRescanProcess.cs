using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Core.Rules.Processors;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.ItemRescan;

public abstract class PipelineRuleRescanProcess : ItemRescanProcessBase
{
    protected readonly IPipelineService _pipelineService;

    protected PipelineRuleRescanProcess(IRuleProcessor ruleProcessor, IProjectService projectService, ICompliancyReportService reportService, IPipelineService pipelineService)
        : base(ruleProcessor, projectService, reportService)
    {
        _pipelineService = pipelineService ?? throw new ArgumentNullException(nameof(pipelineService));
    }

    public async Task RescanAndUpdateReportAsync(PipelineRuleRescanRequest request, CancellationToken cancellationToken = default)
    {
        var rule = _ruleProcessor.GetRuleByName<IPipelineRule>(request.RuleName);
        var itemProject = await GetParentProject(request.Organization, request.ItemProjectId, cancellationToken);
        var pipeline = await GetPipeline(itemProject, request.PipelineId, cancellationToken);
        var result = await rule.EvaluateAsync(pipeline);
        await UpdateReportAsync(request, result, cancellationToken);
    }

    protected abstract Task<Pipeline> GetPipeline(Project project, int scannablePipelineId, CancellationToken cancellationToken = default);
}
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Rules.Processors;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Application.ItemRescan;

public class ClassicReleasePipelineRuleRescanProcess : PipelineRuleRescanProcess, IClassicReleasePipelineRuleRescanProcess
{
    public ClassicReleasePipelineRuleRescanProcess(IRuleProcessor ruleProcessor, IProjectService projectService, ICompliancyReportService reportService, IPipelineService pipelineService)
        : base(ruleProcessor, projectService, reportService, pipelineService)
    {
    }

    protected override async Task<Pipeline> GetPipeline(Project project, int scannablePipelineId, CancellationToken cancellationToken = default)
    {
        return await _pipelineService.GetPipelineAsync(project, scannablePipelineId, PipelineProcessType.DesignerRelease, cancellationToken);
    }
}
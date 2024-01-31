using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.ItemRescan;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Rules.Processors;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Tests.ProcessTestImplementations;

public class PipelineRuleRescanProcessTestImplementation : PipelineRuleRescanProcess, IBuildPipelineRuleRescanProcess
{
    public PipelineRuleRescanProcessTestImplementation(IRuleProcessor ruleProcessor, IProjectService projectService, ICompliancyReportService reportService, IPipelineService pipelineService)
        : base(ruleProcessor, projectService, reportService, pipelineService)
    {
    }

    protected override Task<Pipeline> GetPipeline(Project project, int scannablePipelineId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new Pipeline { Project = new Project { Id = Guid.NewGuid() } });
    }
}
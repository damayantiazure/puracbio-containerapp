using Rabobank.Compliancy.Application.ItemRescan;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Tests.ItemRescan;

public class BuildPipelineRuleRescanProcessTests : PipelineRuleRescanProcessTests
{
    [Fact]
    public async Task GetPipeline_CallsGetPipeline_WithCorrectParameters()
    {
        var cancellationToken = _fixture.Create<CancellationToken>();

        var request = Setup_Default_RescanAndUpdateReportAsync_Mocks();

        var parentProject = Setup_Default_GetProject_Mocks(request.Organization, request.ItemProjectId);
        var pipeline = new Pipeline() { Project = parentProject };
        _pipelineService.Setup(p => p.GetPipelineAsync(parentProject, request.PipelineId, Domain.Enums.PipelineProcessType.Yaml, It.IsAny<CancellationToken>())).ReturnsAsync(pipeline).Verifiable();

        await new BuildPipelineRuleRescanProcess(_ruleProcessor.Object, _projectService.Object, _reportService.Object, _pipelineService.Object)
            .RescanAndUpdateReportAsync(request, cancellationToken);

        _pipelineService.Verify();
    }
}
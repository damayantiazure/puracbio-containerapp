using Rabobank.Compliancy.Application.Requests;

namespace Rabobank.Compliancy.Application.Interfaces;

public interface IBuildPipelineRuleRescanProcess
{
    Task RescanAndUpdateReportAsync(PipelineRuleRescanRequest request, CancellationToken cancellationToken = default);
}
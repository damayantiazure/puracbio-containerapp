

using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Infra.StorageClient.Model;

namespace ComplianceWebApi.Services;

public interface IPipelineBreakerService
{
    Task<PipelineBreakerRegistrationReport> GetPreviousRegistrationResultAsync(PipelineRunInfo runInfo);

    Task<PipelineBreakerReport> GetPreviousComplianceResultAsync(PipelineRunInfo runInfo);

    Task<PipelineRunInfo> EnrichPipelineInfoAsync(PipelineRunInfo runInfo);

    Task<IEnumerable<RuleCompliancyReport>> GetCompliancy(PipelineRunInfo runInfo,
        IEnumerable<PipelineRegistration> registrations);
}
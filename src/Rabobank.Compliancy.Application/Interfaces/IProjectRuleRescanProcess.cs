using Rabobank.Compliancy.Application.Requests;

namespace Rabobank.Compliancy.Application.Interfaces;

public interface IProjectRuleRescanProcess
{
    Task RescanAndUpdateReportAsync(ProjectRuleRescanRequest request, CancellationToken cancellationToken = default);
}
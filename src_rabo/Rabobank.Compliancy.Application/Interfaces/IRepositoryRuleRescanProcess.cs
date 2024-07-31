using Rabobank.Compliancy.Application.Requests;

namespace Rabobank.Compliancy.Application.Interfaces;

public interface IRepositoryRuleRescanProcess
{
    Task RescanAndUpdateReportAsync(RepositoryRuleRescanRequest request, CancellationToken cancellationToken = default);
}
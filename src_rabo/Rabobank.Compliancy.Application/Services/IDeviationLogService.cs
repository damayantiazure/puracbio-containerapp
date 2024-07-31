#nullable enable

using Rabobank.Compliancy.Domain.Compliancy.Deviations;

namespace Rabobank.Compliancy.Application.Services;

public interface IDeviationLogService
{
    Task LogDeviationRecord(DeviationReportLogRecord deviationRecord);
}
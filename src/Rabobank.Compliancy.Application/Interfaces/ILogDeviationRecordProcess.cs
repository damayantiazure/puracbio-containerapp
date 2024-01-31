#nullable enable

namespace Rabobank.Compliancy.Application.Interfaces;

public interface ILogDeviationRecordProcess
{
    Task LogDeviationReportRecord(string deviationRecordData);
}

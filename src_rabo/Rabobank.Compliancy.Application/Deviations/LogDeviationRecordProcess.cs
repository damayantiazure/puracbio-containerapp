#nullable enable

using Newtonsoft.Json;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;

namespace Rabobank.Compliancy.Application.Deviations;

public class LogDeviationRecordProcess : ILogDeviationRecordProcess
{
    private readonly IDeviationLogService _deviationLogService;

    public LogDeviationRecordProcess(IDeviationLogService deviationLogService) => 
        _deviationLogService = deviationLogService;

    public async Task LogDeviationReportRecord(string deviationRecordData)
    {
        var deviationLogRecord = JsonConvert.DeserializeObject<DeviationReportLogRecord>(deviationRecordData);
        if (deviationLogRecord == null)
        {
            return;
        }

        await _deviationLogService.LogDeviationRecord(deviationLogRecord);
    }
}
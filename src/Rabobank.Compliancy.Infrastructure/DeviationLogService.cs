#nullable enable

using AutoMapper;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Infrastructure.Dto.Queue;

namespace Rabobank.Compliancy.Infrastructure;

internal class DeviationLogService : IDeviationLogService
{
    private readonly ILoggingService _loggingService;
    private readonly IMapper _mapper;

    public DeviationLogService(ILoggingService loggingService, IMapper mapper)
    {
        _loggingService = loggingService;
        _mapper = mapper;
    }

    public async Task LogDeviationRecord(DeviationReportLogRecord deviationRecord)
    {
        var deviationQueueDto = _mapper.Map<DeviationQueueDto>(deviationRecord);
        await _loggingService.LogInformationAsync(LogDestinations.DeviationsLog, deviationQueueDto);
    }
}
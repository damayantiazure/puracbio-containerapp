#nullable enable

using AutoMapper;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;
using Rabobank.Compliancy.Infrastructure.Dto.Queue;

namespace Rabobank.Compliancy.Infrastructure.Mapping;

public class DeviationReportLogRecordMappingProfile : Profile
{
    public DeviationReportLogRecordMappingProfile()
    {
        CreateMap<DeviationReportLogRecord, DeviationQueueDto>();
    }
}

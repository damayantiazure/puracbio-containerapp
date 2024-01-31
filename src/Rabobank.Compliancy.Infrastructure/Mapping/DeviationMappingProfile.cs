#nullable enable

using AutoMapper;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;
using Rabobank.Compliancy.Infrastructure.Dto.Queue;
using Deviation = Rabobank.Compliancy.Domain.Compliancy.Deviations.Deviation;

namespace Rabobank.Compliancy.Infrastructure.Mapping;

public class DeviationMappingProfile : Profile
{
    public DeviationMappingProfile()
    {
        CreateMap<Deviation, DeviationQueueDto>();
        CreateMap<Deviation, DeviationReportDto>()
            .ForMember(dest => dest.Reason,
                options => options.MapFrom(src => src.Reason.ToString()))
            .ForMember(dest => dest.ReasonNotApplicable,
                options => options.MapFrom(src => src.ReasonNotApplicable.ToString()));

        CreateMap<Deviation, DeviationReport>()
            .ForMember(dest => dest.Reason,
                options => options.MapFrom(src => src.Reason.ToString()))
            .ForMember(dest => dest.ReasonNotApplicable,
                options => options.MapFrom(src => src.ReasonNotApplicable.ToString()));
    }
}

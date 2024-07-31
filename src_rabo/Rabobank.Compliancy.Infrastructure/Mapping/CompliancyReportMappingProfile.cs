#nullable enable

using AutoMapper;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;

namespace Rabobank.Compliancy.Infrastructure.Mapping;

public class CompliancyReportMappingProfile : Profile
{
    public CompliancyReportMappingProfile()
    {
        CreateMap<CompliancyReport, CompliancyReportDto>().ReverseMap();
        CreateMap<PipelineReport, PipelineReportDto>().ReverseMap();
        CreateMap<CiReport, CiReportDto>()
            .ForMember(dest => dest.Date,
                options =>
                    options.MapFrom(source => source.ScanDate))
            .ReverseMap()
            .ConstructUsing(source => new CiReport(source.Id!, source.Name, source.Date));
        CreateMap<ResourceReport, ResourceReportDto>().ReverseMap();
        CreateMap<NonProdCompliancyReport, NonProdCompliancyReportDto>().ReverseMap();
        CreateMap<StageReport, StageReportDto>().ReverseMap();
        CreateMap<PrincipleReport, PrincipleReportDto>()
            .ForMember(dest => dest.Date,
                options =>
                    options.MapFrom(source => source.ScanDate)).ReverseMap()
            .ConstructUsing(source => new PrincipleReport(source.Name!, source.Date));
        CreateMap<ExceptionSummaryReport, ExceptionSummaryDto>().ReverseMap();
        CreateMap<RuleReport, RuleReportDto>()
            .ForMember(dest => dest.Date,
                options =>
                    options.MapFrom(source => source.ScanDate))
            .ReverseMap()
            .ConstructUsing(source => new RuleReport(source.Name!, source.Date));
        CreateMap<ItemReport, ItemReportDto>()
            .ForMember(dest => dest.Date,
                options =>
                    options.MapFrom(source => source.ScanDate))
            .ReverseMap()
            .ConstructUsing(source => new ItemReport(source.ItemId!, source.Name!, source.ProjectId!, source.Date));
        CreateMap<DeviationReport, DeviationReportDto>().ReverseMap();
        CreateMap<PipelineBreakerRegistrationReport, RegistrationReportDto>().ReverseMap();
        CreateMap<RegistrationPipelineReport, RegistrationReportDto>().ReverseMap();
    }
}

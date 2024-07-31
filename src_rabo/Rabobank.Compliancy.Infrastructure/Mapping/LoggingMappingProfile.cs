#nullable enable

using AutoMapper;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Infrastructure.Dto.Logging;
using Rabobank.Compliancy.Infrastructure.Dto.Queue;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Mapping;

public class LoggingMappingProfile : Profile
{
    [SuppressMessage("Code Smell",
        "S3776:Refactor this constructor to reduce its cognitive complexity.",
        Justification = "Map creation cannot be refactored.")]
    public LoggingMappingProfile()
    {
        CreateMap<AuditLoggingReport, AuditDeploymentLogDto>();
        CreateMap<ExceptionReport, AuditLoggingErrorLogDto>();
        CreateMap<HookFailureReport, AuditLoggingHookFailureLogDto>()
            .ForMember(dest => dest.ProjectId, option => 
            option.MapFrom(source => source.ProjectId));
        CreateMap<PoisonMessageReport, AuditPoisonMessagesLogDto>();
        CreateMap<AuditLoggingPullRequestReport, AuditPullRequestApproversLogDto>();
        CreateMap<ExceptionReport, ComplianceScannerOnlineErrorLogDto>();
        CreateMap<CiReport, CompliancyCisDto>();
        CreateMap<ItemReport, CompliancyItemsDto>()
            .ForMember(source => source.DeviationComment, options =>
                options.MapFrom(source => source.Deviation == null ? null : source.Deviation.Comment))
            .ForMember(source => source.DeviationReason, options =>
                options.MapFrom(source => source.Deviation == null ? null : source.Deviation.Reason))
            .ForMember(source => source.DeviationReasonNotApplicable, options =>
                options.MapFrom(source => source.Deviation == null ? null : source.Deviation.ReasonNotApplicable))
            .ForMember(source => source.DeviationReasonNotApplicableOther, options =>
                options.MapFrom(source => source.Deviation == null ? null : source.Deviation.ReasonNotApplicableOther))
            .ForMember(source => source.DeviationReasonOther, options =>
                options.MapFrom(source => source.Deviation == null ? null : source.Deviation.ReasonOther))
            .ForMember(source => source.DeviationUpdatedBy, options =>
                options.MapFrom(source => source.Deviation == null ? null : source.Deviation.UpdatedBy));
        CreateMap<CompliancyPipelineReport, CompliancyPipelinesDto>();
        CreateMap<PrincipleReport, CompliancyPrinciplesDto>();
        CreateMap<RuleReport, CompliancyRulesDto>();
        CreateMap<DecoratorErrorReport, DecoratorErrorLogDto>();
        CreateMap<DeviationQueueDto, DeviationsLogDto>();
        CreateMap<ExceptionReport, ErrorHandlingLogDto>();
        CreateMap<ExceptionReport, Sm9ChangesErrorLogDto>();
        CreateMap<ExceptionReport, ValidateGatesErrorLogDto>();
        CreateMap<PipelineBreakerReport, PipelineBreakerComplianceLogDto>();
        CreateMap<RuleCompliancyReport, RuleCompliancyLogDto>();
        CreateMap<ExceptionReport, PipelineBreakerErrorLogDto>();
        CreateMap<PipelineBreakerRegistrationReport, PipelineBreakerLogDto>();
    }
}
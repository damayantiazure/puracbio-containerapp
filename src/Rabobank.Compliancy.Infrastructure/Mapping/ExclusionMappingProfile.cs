#nullable enable

using AutoMapper;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.Exclusions;
using Rabobank.Compliancy.Domain.Compliancy.Exclusions;

namespace Rabobank.Compliancy.Infrastructure.Mapping;

public class ExclusionMappingProfile : Profile
{
    public ExclusionMappingProfile()
    {
        CreateMap<Exclusion, ExclusionEntity>()
            .ConstructUsing(src => new ExclusionEntity(src.Organization, src.ProjectId, src.PipelineId, src.PipelineType))
            .ReverseMap();
    }
}
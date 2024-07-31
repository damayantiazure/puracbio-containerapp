using AutoMapper;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Infrastructure.AzureDevOps.Mapping;

/// Class intended to map a <see cref="Pipeline"/> to an <see cref="AzdoReleaseDefinitionPipeline"/> while these are still mostly the same class and primarily used for Generic differentiation
public class PipelineToAzdoReleaseDefinitionProfile : Profile
{
    public PipelineToAzdoReleaseDefinitionProfile()
    {
        CreateMap<Pipeline, AzdoReleaseDefinitionPipeline>();
    }
}

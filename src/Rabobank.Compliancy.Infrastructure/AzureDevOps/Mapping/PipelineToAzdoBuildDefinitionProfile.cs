using AutoMapper;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Infrastructure.AzureDevOps.Mapping;

/// <summary>
/// Class intended to map a <see cref="Pipeline"/> to an <see cref="AzdoBuildDefinitionPipeline"/> while these are still mostly the same class and primarily used for Generic differentiation 
/// </summary>
public class PipelineToAzdoBuildDefinitionProfile : Profile
{
    public PipelineToAzdoBuildDefinitionProfile()
    {
        CreateMap<Pipeline, AzdoBuildDefinitionPipeline>();
    }
}

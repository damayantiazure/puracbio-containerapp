using AutoMapper;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;

namespace Rabobank.Compliancy.Infrastructure.Mapping;

internal class UpdatePermissionEntityMappingProfile : Profile
{
    public UpdatePermissionEntityMappingProfile()
    {
        CreateMap<Permission, UpdatePermissionEntity>()
            .ForMember(dest => dest.Token,
                options => options.MapFrom(src => src.PermissionToken))
            .ForMember(dest => dest.PermissionId,
                options => options.MapFrom(src => src.PermissionId))
            .ForMember(dest => dest.NamespaceId,
                options => options.MapFrom(src => src.NamespaceId))
            .ForMember(dest => dest.PermissionBit,
                options => options.MapFrom(src => src.PermissionBit));
    }
}
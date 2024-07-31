using AutoMapper;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;

namespace Rabobank.Compliancy.Infrastructure.Mapping;

public class UpdatePermissionBodyMappingProfile : Profile
{
    public UpdatePermissionBodyMappingProfile()
    {
        CreateMap<PermissionsSet, UpdatePermissionBody>()
          .ForMember(dest => dest.DescriptorIdentifier,
              options => options.MapFrom(src => src.DescriptorIdentifier))
          .ForMember(dest => dest.DescriptorIdentityType,
              options => options.MapFrom(src => src.DescriptorIdentityType))
          .ForMember(dest => dest.PermissionSetId,
              options => options.MapFrom(src => src.Permissions.First().NamespaceId));
    }
}
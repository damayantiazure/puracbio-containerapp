using Newtonsoft.Json;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Linq;
using static Rabobank.Compliancy.Infra.AzdoClient.Requests.Permissions;

namespace Rabobank.Compliancy.Infra.AzdoClient.Permissions;

public class ManagePermissionsData
{
    public ManagePermissionsData(string tfid, string descriptorIdentifier,
        string descriptorIdentityType, string token, params Permission[] permissions)
    {
        TeamFoundationId = tfid;
        DescriptorIdentityType = descriptorIdentityType;
        DescriptorIdentifier = descriptorIdentifier;
        Updates = permissions.Select(permission => new
        {
            Token = permission.PermissionToken,
            permission.PermissionId,
            permission.NamespaceId,
            permission.PermissionBit
        });
        PermissionSetId = permissions.First().NamespaceId;
        PermissionSetToken = token;
    }

    public object Updates { get; }

    public string TeamFoundationId { get; }
    public string PermissionSetId { get; }
    public string PermissionSetToken { get; }
    public string DescriptorIdentityType { get; }
    public string DescriptorIdentifier { get; }
    public bool RefreshIdentities { get; }
    public bool IsRemovingIdentity { get; }
    public string TokenDisplayName { get; }

    public UpdateWrapper Wrap() =>
        new UpdateWrapper(JsonConvert.SerializeObject(this));
}
using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class PermissionsSet
{
    public IEnumerable<Permission> Permissions { get; set; }
    public string CurrentTeamFoundationId { get; set; }
    public string DescriptorIdentifier { get; set; }
    public string DescriptorIdentityType { get; set; }
}
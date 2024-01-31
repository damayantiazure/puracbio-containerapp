using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Domain.Compliancy;

public class Permission
{
    public string Name { get; set; }
    public PermissionType Type { get; set; }
}
namespace Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;

public static class PermissionLevelId
{
    public const int NotSet = 0;
    public const int Allow = 1;
    public const int Deny = 2;
    public const int AllowInherited = 3;
    public const int DenyInherited = 4;
}
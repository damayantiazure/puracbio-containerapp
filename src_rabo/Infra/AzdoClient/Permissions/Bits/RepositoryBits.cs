namespace Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;

public static class RepositoryBits
{
    public const int Contribute = 4;
    public const int ForcePush = 8;
    public const int BypassPoliciesCodePush = 128;
    public const int DeleteRepository = 512;
    public const int ManagePermissions = 8192;
    public const int BypassPoliciesPullRequest = 32768;
}
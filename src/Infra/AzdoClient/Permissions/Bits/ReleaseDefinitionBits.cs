namespace Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;

public static class ReleaseDefinitionBits
{
    public const int ViewReleasePipeline = 1;
    public const int EditReleasePipeline = 2;
    public const int DeleteReleasePipelines = 4;
    public const int ManageApprovals = 8;
    public const int CreateReleases = 64;
    public const int EditReleaseStage = 128;
    public const int AdministerReleasePermissions = 512;
    public const int DeleteReleases = 1024;
}
namespace Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;

public static class BuildDefinitionBits
{
    public const int DeleteBuilds = 8;
    public const int DestroyBuilds = 32;
    public const int QueueBuilds = 128;
    public const int DeleteBuildDefinition = 4096;
    public const int AdministerBuildPermissions = 16384;
    public const int EditBuildPipeline = 2048;
}
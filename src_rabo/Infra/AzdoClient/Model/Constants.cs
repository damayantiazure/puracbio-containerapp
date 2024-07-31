namespace Rabobank.Compliancy.Infra.AzdoClient.Model;

public static class Constants
{
    public static class Organization
    {
        public const string DefaultOrganization = "raboweb";
    }

    public static class AzureDevOpsGroups
    {
        public const string ProjectCollectionAdministrators = "Project Collection Administrators";
        public const string ProjectCollectionBuildAdministrators = "Project Collection Build Administrators";
        public const string ProductionEnvironmentOwners = "Production Environment Owners";
        public const string ProjectCollectionServiceAccounts = "Project Collection Service Accounts";
        public const string ProjectAdministrators = "Project Administrators";
        public const string RabobankProjectAdministrators = "Rabobank Project Administrators";
        public const string BuildAdministrators = "Build Administrators";
        public const string ReleaseAdministrators = "Release Administrators";
        public const string Contributors = "Contributors";
    }

    public static class PipelineProcessType
    {
        public const int GuiPipeline = 1;
        public const int YamlPipeline = 2;
    }

    public static class HostTypes
    {
        /// <summary>
        /// Hosttype when running from a yaml build
        /// </summary>
        public const string Build = "build";
        /// <summary>
        /// Hosttype when running from a check function on an environment
        /// </summary>
        public const string Checks = "checks";
    }

    public static class ItemTypes
    {
        public const string Repository = "Repository";
        public const string BuildPipeline = "Build";
        public const string ReleasePipeline = "Release";
        public const string YamlReleasePipeline = "YAML release";
        public const string ClassicReleasePipeline = "Classic release";
        public const string Project = "Project";
        public const string DisabledYamlPipeline = "Disabled YAML pipeline";
        public const string InvalidYamlPipeline = "Invalid YAML pipeline";
        public const string StagelessYamlPipeline = "Stageless YAML pipeline";
        public const string YamlPipelineWithStages = "YAML pipeline with stages";
        public const string ClassicBuildPipeline = "Classic build pipeline";
        public const string Dummy = "Dummy";
        public const int ClassicPipeline = 1;
        public const int YamlPipeline = 2;
    }

    public static class RepositoryTypes
    {
        public const string Git = "git";
        public const string TfsGit = "TfsGit";
    }
}
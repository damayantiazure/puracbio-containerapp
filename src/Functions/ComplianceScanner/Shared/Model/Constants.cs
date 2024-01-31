namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;

public static class Constants
{
    public static class ExclusionConstants
    {
        public const byte HoursValid = 24;
    }

    public static class PreviewFeatures
    {
        public const string PipelineBreakerCompliancy = "PipelineBreakerCompliancy";
    }

    public static class ConfluenceLinks
    {
        public const string InvalidPipelines = "https://confluence.dev.rabobank.nl/x/2GflCw#AzureDevOpsComplianceHub-Troubleshooting:InvalidYAMLpipelines";
        public const string CompliancyDocumentation = "https://confluence.dev.rabobank.nl/x/2GflCw#AzureDevOpsComplianceHub-HOW-TO:Becomecompliant";
        public const string RegisterPipelines = "https://confluence.dev.rabobank.nl/x/2GflCw#AzureDevOpsComplianceHub-HOW-TO:Pipelineregistrations";            
    }

    public enum DecoratorPrefix
    {
        WARNING,
        ERROR
    }

    public static class DecoratorErrors
    {
        public const string ErrorPrefix = "[PipelineBreakerError]";
    }

    public static class DecoratorResultMessages
    {
        public const string Passed = "This pipeline is allowed to continue.";

        public const string NotRegistered = @$"This pipeline is not registered and as of May 1st, 2022 unregistered pipelines are being blocked. 
Here you can find how to register your pipeline: {ConfluenceLinks.RegisterPipelines} ";

        public const string AlreadyScanned = "This pipeline run has already been scanned during a previous job, which resulted in a pass.";

        public const string WarningAlreadyScanned = @$"{nameof(DecoratorPrefix.WARNING)}: This pipeline run has already been scanned during a previous job, which resulted in a warning. 
Please check the first warning in this pipeline run for the detailed message.";

        public const string InvalidYaml = $@"This yaml pipeline could not be parsed, because it is invalid. Therefore, a compliance scan could not be performed.
As of May 1st, 2022 invalid pipelines are being blocked. 
Here you can find how to troubleshoot invalid pipelines: {ConfluenceLinks.InvalidPipelines} ";

        public const string ExclusionList = "This pipeline is on the exclusion list and is therefore allowed to continue.";

        public const string NotCompliant = $"{nameof(DecoratorPrefix.ERROR)}: This pipeline is not compliant and will be blocked. ";

        public const string WarningNotCompliant = $"{nameof(DecoratorPrefix.WARNING)}: This pipeline is not compliant and as of September 1st, runs will be blocked. ";

        public const string NoProdStagesFound = @"Your pipeline is registered as a PROD pipeline and none of the PROD stages registered in the CMDB are present in the current version of your pipeline. 
For the Compliancy Rules to be able to be evaluated at least 1 of the registered stages needs to be present.";
    }
}
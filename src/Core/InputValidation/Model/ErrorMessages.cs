using System;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Core.InputValidation.Model;

public static class ErrorMessages
{
    private const string ClassicDocumentationLink = "https://confluence.dev.rabobank.nl/x/vNCMEQ";
    private const string YamlDocumentationLink = "https://confluence.dev.rabobank.nl/x/NYoVEg";
    private const string NoApprovalErrorMessage =
        @"Neither a pull request approval nor a pipeline approval has been provided.
            ---------------------------------------------------------------------------------------------
            To be compliant, you either need an approval on the pipeline or on the pull-request. 
            Please check the following:
            - Is there an approval on the pull-request?
                - If so, is the approver not the one who completed the pull-request?
            - Is there an approval on the pipeline?
                - If so, is the approver not the one who started the pipeline?
            More information can be found here:";
    private const string ArgumentExceptionErrorMessage =
        @"An invalid project ID and/or release/run ID was provided.
            ---------------------------------------------------------------------------------------------
            Project IDs should be GUIDs and release/run IDs should be numbers.
            Azure Function syntax:
            - For (YAML-pipeline) Runs: https://validategatesprd.azurewebsites.net/api/validate-yaml-approvers/(System.TeamProjectId)/(Build.BuildId)
            - For (Classic Pipeline) Releases: https://validategatesprd.azurewebsites.net/api/validate-classic-approvers/(System.TeamProjectId)/(Release.ReleaseId)
            Below exception will tell you whether the problem lies with your provided project ID and/or release/run ID.
            ---------------------------------------------------------------------------------------------
            The following exception has been thrown:";
    private const string InternalServerErrorMessage =
        @"An unexpected error occurred during the validation of the gate.
            ---------------------------------------------------------------------------------------------
            Please run your pipeline again and if it occurs more frequently please reach out to Tech4Dev.
            Tech4Dev can be reached by creating a support ticket via https://tools.rabobank.nl.
            ---------------------------------------------------------------------------------------------
            The following exception has been thrown:";
    private const string OverwriteClassicGatesMessage =
        @"---------------------------------------------------------------------------------------------
            In case of emergencies the gate outcome can be overwritten by your Production Environment Owner.
            Furthermore the gate can also be disabled by your Production Environment Owner.
            More information can be found here: 
            https://confluence.dev.rabobank.nl/x/vNCMEQ#Classicproductiondeploymentisblockedwithout4-eyesapproval-RollbackorIgnore";
    private const string RemoveYamlGatesMessage =
        @"---------------------------------------------------------------------------------------------
            In case of emergencies the gate can be removed by your Production Environment Owner.
            More information can be found here: 
            https://confluence.dev.rabobank.nl/x/NYoVEg#YAMLproductiondeploymentisblockedwithout4-eyesapproval-Rollback";

    public static string CreateNoApprovalErrorMessage(int pipelineType) =>
        pipelineType switch
        {
            ItemTypes.ClassicPipeline =>
                $@"{NoApprovalErrorMessage} {ClassicDocumentationLink}
                    {OverwriteClassicGatesMessage}",
            ItemTypes.YamlPipeline =>
                $@"{NoApprovalErrorMessage} {YamlDocumentationLink}
                    {RemoveYamlGatesMessage}",
            _ => throw new NotImplementedException(),
        };

    public static string CreateArgumentExceptionErrorMessage(string exceptionMessage) =>
        $@"{ArgumentExceptionErrorMessage}
            {exceptionMessage}";

    public static string CreateInternalServerErrorMessage(int pipelineType, string exceptionMessage) =>
        pipelineType switch
        {
            ItemTypes.ClassicPipeline =>
                $@"{InternalServerErrorMessage} 
                    {exceptionMessage}
                    {OverwriteClassicGatesMessage}",
            ItemTypes.YamlPipeline =>
                $@"{InternalServerErrorMessage} 
                    {exceptionMessage}
                    {RemoveYamlGatesMessage}",
            _ => throw new NotImplementedException(),
        };
}
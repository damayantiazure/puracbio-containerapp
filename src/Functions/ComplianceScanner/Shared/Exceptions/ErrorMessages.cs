namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Exceptions;

public static class ErrorMessages
{
    public const string InvalidApprover =
        @"The approval of your exclusion request failed. 
The approver is the same person as the requester.
In order to request a valid exclusion, 4-eyes is required.";

    public const string AlreadyApproved =
        @"There already is a valid exclusion for this pipeline.";

    public const string ItemNotFoundError =
        @"An error occurred while retrieving the permissions for this pipeline.
This item could not be found and has probably already been removed from Azure DevOps. 
Please rescan the project to make sure deleted items are removed from the overview.";

    public static string FinalYamlCouldNotBeRetrieved(string pipelineId, string pipelineName) =>
        $"Unable to retrieve yaml for pipeline '{pipelineName}({pipelineId})'. Please verify if this pipeline is valid.";
}
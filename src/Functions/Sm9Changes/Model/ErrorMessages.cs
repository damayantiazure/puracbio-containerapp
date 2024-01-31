using Rabobank.Compliancy.Functions.Sm9Changes.Extensions;
using Rabobank.Compliancy.Infra.Sm9Client.Change.Model;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Model;

public static class ErrorMessages
{
    public static string InvalidHeaderException(string headerName, string requiredHeaderValue) =>
        $@"'{headerName}' was not provided in the request headers.
The {headerName} can be provided by adding the following to your request headers:
{headerName}: {requiredHeaderValue}";

    public static string InvalidHeaderException(string headerName, string requiredHeaderValue, 
        string actualHeaderValue) =>
        $@"An invalid '{headerName}' was provided in the request headers: {headerName}: {actualHeaderValue}. 
The {headerName} can be provided by adding the following to your request headers:
{headerName}: {requiredHeaderValue}";

    public static string InvalidHeaderException(string headerName1, string requiredHeaderValue1, 
        string headerName2, string requiredHeaderValue2) =>
        $@"For either '{headerName1}' or '{headerName2}' an invalid value has been provided in the request headers.
The {headerName1} can be provided by adding the following to your request headers:
{headerName1}: {requiredHeaderValue1}
The {headerName2} can be provided by adding the following to your request headers:
{headerName2}: {requiredHeaderValue2}";

    public static string InvalidHeader(string exceptionMessage, string pipelineType, 
        bool isLowRiskTask, HttpRequestMessage request) =>
        $@"The input provided via the request headers is incomplete or invalid.
---------------------------------------------------------------------------------------------
Please have a look at the documentation to verify the correct request headers: 
{GetDocumentationLink(pipelineType, isLowRiskTask)}
---------------------------------------------------------------------------------------------
The following exception has been thrown:
{exceptionMessage}
{LogRequestDetails(request)}";
        
    public static string ChangeIdNotFound(string pipelineType, bool isLowRiskTask, 
        HttpRequestMessage request = null) =>
        $@"No valid ChangeId has been provided.
---------------------------------------------------------------------------------------------
Please have a look at the documentation to verify how to provide a Change Id in your pipeline run: 
{GetDocumentationLink(pipelineType, isLowRiskTask)}
---------------------------------------------------------------------------------------------
The following exception has been thrown:
No valid Change Id has been provided via either pipeline tags or pipeline variables.
The required format for a ChangeId is: C123456789.
{LogRequestDetails(request)}";

    public static string InvalidCompletionCode(string pipelineType, bool isLowRiskTask, 
        string completionCode) =>
        $@"The provided ClosureCode: '{completionCode}' is invalid.
---------------------------------------------------------------------------------------------
Please have a look at the documentation to verify the input for the Close Change Task: 
{GetDocumentationLink(pipelineType, isLowRiskTask)}
---------------------------------------------------------------------------------------------
The following exception has been thrown:
Invalid value for ClosureCode. ClosureCode must be between 1 and 6.";

    public static string ApproverNotFound(string pipelineType, bool isLowRiskTask) =>
        $@"Neither a pull request approval nor a pipeline approval has been provided.
---------------------------------------------------------------------------------------------
To use this task you either need an approval on the pipeline or on the pull-request. 
Please have a look at the documentation for more information: 
{GetDocumentationLink(pipelineType, isLowRiskTask)}
---------------------------------------------------------------------------------------------
The following exception has been thrown:
No approval has been found.";

    public static string PipelineUrlNotFound(string organization, string projectId, string runId) =>
        $@"Pipeline url could not be found: organization={organization}, projectId={projectId}, runId={runId}";

    public static string InitiatorNotFound() =>
        $@"No pipeline initiator could be found.";

    public static string InvalidChangePhase(IEnumerable<ChangeInformation> changeDetails,
        string pipelineType, bool isLowRiskTask, HttpRequestMessage request = null)
    {
        if (isLowRiskTask)
        {
            return $@"The following Changes do not have the correct Change Phase: {changeDetails.Select(c => c.ChangeId).ToCommaSeparatedString()}.
{ChangeDetails(changeDetails)}
---------------------------------------------------------------------------------------------
Something went wrong during validation of the Change(s). The Change Phase should be '{SM9Constants.DeploymentPhase}'.
If it occurs more frequently please reach out to the SM9 Team. Here you can find how to reach SM9 Team: 
{SM9Constants.ClassicLowRiskDocumentationLink}
---------------------------------------------------------------------------------------------
In case of emergencies you can disable the SM9 task in your pipeline or enable continueOnError on the task.
Note that in this case the change is not created/approved/closed from your pipeline and needs to be created/closed manually in SM9.
---------------------------------------------------------------------------------------------";
        }
        else
        {
            return $@"The following Changes do not have the correct Change Phase: {changeDetails.Select(c => c.ChangeId).ToCommaSeparatedString()}.
{ChangeDetails(changeDetails)}
---------------------------------------------------------------------------------------------
Please have a look at the documentation for more information: 
{GetDocumentationLink(pipelineType, isLowRiskTask)}
---------------------------------------------------------------------------------------------
The following exception has been thrown:
Something went wrong during validation of the Change(s). The Change Phase should be '{SM9Constants.DeploymentPhase}'.
{LogRequestDetails(request)}";
        }
    }

    public static string ChangeIdNotReceived(CreateChangeResponse response) =>
        $@"No Change ID received from SM9 Create Change API call. 
Response message = {response.Messages}
Return code = {response.ReturnCode}";

    public static string ChangeUrlNotReceived(GetChangeByKeyResponse response) =>
        $@"No Change URL with hash received from SM9 GetChangeByKey API call. 
Response message = { response.Messages }
Return code = { response.ReturnCode }";

    public static string Sm9TeamError(string exceptionMessage, HttpRequestMessage request = null) =>
        $@"An unexpected error occurred during the api call to SM9.
---------------------------------------------------------------------------------------------
Please run your pipeline again and if it occurs more frequently please reach out to the SM9 Team.
Here you can find how to reach SM9 Team: {SM9Constants.ClassicLowRiskDocumentationLink} 
---------------------------------------------------------------------------------------------
In case of emergencies you can disable the SM9 task in your pipeline or enable continueOnError on the task.
Note that in this case the change is not created/approved/closed from your pipeline and needs to be created manually in SM9.
---------------------------------------------------------------------------------------------
The following exception has been thrown:
{exceptionMessage}
{LogRequestDetails(request)}";

    public static string InternalServerError(string exceptionMessage, HttpRequestMessage request = null) =>
        $@"An unexpected error occurred during the SM9 change task or gate.
---------------------------------------------------------------------------------------------
Please run your pipeline again and if it occurs more frequently please reach out to Tech4Dev.
Tech4Dev can be reached by creating a support ticket via https://tools.rabobank.nl.
---------------------------------------------------------------------------------------------
In case of emergencies you can disable the SM9 task in your pipeline or enable continueOnError on the task.
Note that in this case the change is not created/approved/closed from your pipeline and needs to be created manually in SM9.
---------------------------------------------------------------------------------------------
The following exception has been thrown:
{exceptionMessage}
{LogRequestDetails(request)}";

    private static string GetDocumentationLink(string pipelineType, bool isLowRiskTask)
    {
        if (!isLowRiskTask)
        {
            return SM9Constants.MediumHighRiskDocumentationLink;
        }

        return pipelineType == SM9Constants.BuildPipelineType
            ? SM9Constants.YamlLowRiskDocumentationLink
            : SM9Constants.ClassicLowRiskDocumentationLink;
    }

    private static string LogRequestDetails(HttpRequestMessage request) =>
        request == null
            ? null
            : $@"---------------------------------------------------------------------------------------------
Request URL = {request?.RequestUri?.AbsoluteUri}
Request Headers: 
    PlanUrl = {request?.Headers?.PlanUrl()}
    ProjectId = {request?.Headers?.ProjectId()}
    BuildId = {request?.Headers?.BuildId()}
    Release = {request?.Headers?.ReleaseId()}";

    private static string ChangeDetails(IEnumerable<ChangeInformation> changes)
    {
        StringBuilder message = new();

        foreach (var change in changes) 
        {
            message.AppendLine($@"Change '{change.ChangeId}' is in Change Phase: '{change.Phase}'.");
        }

        return message.ToString();
    }
}
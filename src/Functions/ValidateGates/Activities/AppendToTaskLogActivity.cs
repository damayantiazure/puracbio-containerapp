#nullable enable

using Flurl.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Rabobank.Compliancy.Functions.ValidateGates.Model;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Functions.ValidateGates.Activities;

public class AppendToTaskLogActivity
{
    private readonly IAzdoRestClient _azdoClient;

    public AppendToTaskLogActivity(IAzdoRestClient azdoClient) => _azdoClient = azdoClient;

    [FunctionName(nameof(AppendToTaskLogActivity))]
    public async Task RunAsync([ActivityTrigger] (ValidateApproversAzdoData data, string message) input)
    {
        try
        {
            var azdoData = input.data;
            var taskLogBody = DistributedTask.CreateGetTaskLogBody(azdoData.TaskInstanceId);

            if (azdoData.HubName == HostTypes.Build || azdoData.HubName == HostTypes.Checks)
            {
                await PostMessageForYamlRun(azdoData, taskLogBody, input.message);
            }
            else
            {
                await PostMessageForClassicRun(azdoData, taskLogBody, input.message);
            }
        }
        catch (FlurlHttpException ex)
        {
            throw await ex.MakeDurableFunctionCompatible();
        }
    }

    private async Task PostMessageForYamlRun(ValidateApproversAzdoData azdoData, object taskLogBody, string message)
    {
        var taskLog = await _azdoClient.PostWithCustomTokenAsync(DistributedTask.GetTaskLog(azdoData.ProjectIdCallback,
        azdoData.HubName, azdoData.PlanId), taskLogBody, azdoData.Token, azdoData.Organization, true);

        await _azdoClient.PostStringAsHttpContentAsync(DistributedTask.AppendToTaskLog(azdoData.ProjectIdCallback,
            azdoData.HubName, azdoData.PlanId, taskLog.Id), message, azdoData.Token, azdoData.Organization, true);
    }

    private async Task PostMessageForClassicRun(ValidateApproversAzdoData azdoData, object taskLogBody, string message)
    {
        var taskLog = await _azdoClient.PostWithCustomTokenAsync(DistributedTaskClassic.GetTaskLog(azdoData.ProjectIdCallback,
            azdoData.HubName, azdoData.PlanId), taskLogBody, azdoData.Token, azdoData.Organization, true);

        await _azdoClient.PostStringAsHttpContentAsync(DistributedTaskClassic.AppendToTaskLog(azdoData.ProjectIdCallback,
            azdoData.HubName, azdoData.PlanId, taskLog.Id), message, azdoData.Token, azdoData.Organization, true);
    }
}
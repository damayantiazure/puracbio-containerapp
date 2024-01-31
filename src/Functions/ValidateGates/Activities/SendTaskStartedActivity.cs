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

public class SendTaskStartedActivity
{
    private readonly IAzdoRestClient _azdoClient;

    public SendTaskStartedActivity(IAzdoRestClient azdoClient)
    {
        _azdoClient = azdoClient;
    }

    [FunctionName(nameof(SendTaskStartedActivity))]
    public async Task RunAsync([ActivityTrigger] ValidateApproversAzdoData input)
    {
        try
        {
            var taskStartedBody = DistributedTask.CreateTaskStartedBody(input.JobId, input.TaskInstanceId);

            if (input.HubName == HostTypes.Build || input.HubName == HostTypes.Checks)
            {
                await _azdoClient.PostWithCustomTokenAsync(DistributedTask.TaskStartedEvent(input.ProjectIdCallback, input.HubName, input.PlanId),
                    taskStartedBody, input.Token, input.Organization, true);
            }
            else
            {
                await _azdoClient.PostWithCustomTokenAsync(DistributedTaskClassic.TaskStartedEvent(input.ProjectIdCallback, input.HubName, input.PlanId), taskStartedBody,
                    input.Token, input.Organization, true);
            }
        }
        catch (FlurlHttpException ex)
        {
            throw await ex.MakeDurableFunctionCompatible();
        }
    }
}
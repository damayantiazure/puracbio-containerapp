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

public class SendTaskCompletedActivity
{
    private readonly IAzdoRestClient _azdoClient;

    public SendTaskCompletedActivity(IAzdoRestClient azdoClient) => _azdoClient = azdoClient;

    [FunctionName(nameof(SendTaskCompletedActivity))]
    public async Task RunAsync([ActivityTrigger] (ValidateApproversAzdoData data, bool hasApproval) input)
    {
        try
        {
            var azdoData = input.data;
            var completedBody = DistributedTask.CreateTaskCompletedBody(input.hasApproval, azdoData.JobId, azdoData.TaskInstanceId);

            if (azdoData.HubName == HostTypes.Build || azdoData.HubName == HostTypes.Checks)
            {
                await _azdoClient.PostWithCustomTokenAsync(DistributedTask.TaskCompletedEvent(azdoData.ProjectIdCallback, azdoData.HubName, azdoData.PlanId),
                    completedBody, azdoData.Token, azdoData.Organization, true);
            }
            else
            {
                await _azdoClient.PostWithCustomTokenAsync(DistributedTaskClassic.TaskCompletedEvent(azdoData.ProjectIdCallback, azdoData.HubName, azdoData.PlanId),
                    completedBody, azdoData.Token, azdoData.Organization, true);
            }
        }
        catch (FlurlHttpException ex)
        {
            throw await ex.MakeDurableFunctionCompatible();
        }
    }
}
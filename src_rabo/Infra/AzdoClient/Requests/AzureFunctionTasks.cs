using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class AzureFunctionTasks
{
    public static IAzdoRequest<object, JObject> CreateSignalCompletionRequest(
        string projectId,
        string hubName,
        string planId) =>
        new AzdoRequest<object, JObject>(
            $"{projectId}/_apis/distributedtask/hubs/{hubName}/plans/{planId}/events",
            new Dictionary<string, object>
            {
                {"api-version", "2.0-preview.1"}
            });

    public static object CreateSignalCompletionBody(    
        string taskInstanceId,
        string jobId,
        bool success) =>
        new
        {
            name = "TaskCompleted",
            taskId = taskInstanceId,
            jobId,
            result = success ? "succeeded" : "failed"
        };
}
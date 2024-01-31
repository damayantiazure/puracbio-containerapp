using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class DistributedTask
{
    public static IEnumerableRequest<AgentPoolInfo> OrganizationalAgentPools() => 
        new AzdoRequest<AgentPoolInfo>("_apis/distributedtask/pools").AsEnumerable();

    public static IAzdoRequest<AgentPoolInfo> AgentPool(int id) => 
        new AzdoRequest<AgentPoolInfo>($"_apis/distributedtask/pools/{id}");

    public static IEnumerableRequest<AgentStatus> AgentPoolStatus(int id) =>
        new AzdoRequest<AgentStatus>($"_apis/distributedtask/pools/{id}/agents", new Dictionary<string, object>
        {
            {"includeCapabilities", "false"},
            {"includeAssignedRequest", "true"}
        }).AsEnumerable();

    public static IEnumerableRequest<Task> Tasks() => 
        new AzdoRequest<Task>("_apis/distributedtask/tasks").AsEnumerable();

    public static IAzdoRequest<AgentQueue> AgentQueue(string project, int id) => 
        new AzdoRequest<AgentQueue>($"/{project}/_apis/distributedtask/queues/{id}");


    public static IAzdoRequest<object, JObject> TaskStartedEvent(string project, string hubName, string planId) =>
        new AzdoRequest<object, JObject>($"/{project}/_apis/distributedtask/hubs/{hubName}/plans/{planId}/events",
            new Dictionary<string, object>
            {
                {"api-version", "2.0-preview.1"}
            });

    public static IAzdoRequest<object, JObject> TaskCompletedEvent(string project, string hubName, string planId) =>
        new AzdoRequest<object, JObject>($"/{project}/_apis/distributedtask/hubs/{hubName}/plans/{planId}/events",
            new Dictionary<string, object>
            {
                {"api-version", "2.0-preview.1"}
            });

    public static object CreateTaskStartedBody(string jobId, string taskInstanceId) =>
        new { name = "TaskStarted", jobId = jobId, taskId = taskInstanceId };

    public static IAzdoRequest<object, TaskLog> GetTaskLog(string project, string hubName, string planId) =>
        new AzdoRequest<object, TaskLog>($"/{project}/_apis/distributedtask/hubs/{hubName}/plans/{planId}/logs",
            new Dictionary<string, object>
            {
                {"api-version", "4.1"}
            });

    public static object CreateGetTaskLogBody(string taskInstanceId) =>
        new { path = string.Format(@$"logs\{taskInstanceId:D}") };

    public static IAzdoRequest<object, JObject> AppendToTaskLog(string project, string hubName, string planId, string taskLogId) =>
        new AzdoRequest<object, JObject>($"/{project}/_apis/distributedtask/hubs/{hubName}/plans/{planId}/logs/{taskLogId}",
            new Dictionary<string, object>
            {
                {"api-version", "4.1"}
            });

    public static object CreateTaskCompletedBody(bool successResult, string jobId, string taskInstanceId) =>
        new { result = successResult ? "succeeded" : "failed", jobId = jobId, taskId = taskInstanceId }; 
}
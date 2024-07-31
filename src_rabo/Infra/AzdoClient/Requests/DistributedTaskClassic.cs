using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class DistributedTaskClassic
{
    public static IAzdoRequest<object, JObject> TaskStartedEvent(string project, string hubName, string planId) =>
        new VsrmRequest<object, JObject>($"/{project}/_apis/distributedtask/hubs/{hubName}/plans/{planId}/events",
            new Dictionary<string, object>
            {
                {"api-version", "2.0-preview.1"}
            });

    public static IAzdoRequest<object, JObject> TaskCompletedEvent(string project, string hubName, string planId) =>
        new VsrmRequest<object, JObject>($"/{project}/_apis/distributedtask/hubs/{hubName}/plans/{planId}/events",
            new Dictionary<string, object>
            {
                {"api-version", "2.0-preview.1"}
            });

    public static IAzdoRequest<object, TaskLog> GetTaskLog(string project, string hubName, string planId) =>
        new VsrmRequest<object, TaskLog>($"/{project}/_apis/distributedtask/hubs/{hubName}/plans/{planId}/logs",
            new Dictionary<string, object>
            {
                {"api-version", "4.1"}
            });

    public static IAzdoRequest<object, JObject> AppendToTaskLog(string project, string hubName, string planId, string taskLogId) =>
        new VsrmRequest<object, JObject>($"/{project}/_apis/distributedtask/hubs/{hubName}/plans/{planId}/logs/{taskLogId}",
            new Dictionary<string, object>
            {
                {"api-version", "4.1"}
            });
}
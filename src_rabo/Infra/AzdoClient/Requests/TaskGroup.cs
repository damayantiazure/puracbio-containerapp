using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class TaskGroup
{
    public static IAzdoRequest<Response.TaskGroupResponse> TaskGroupById(string project, string id) =>
        new AzdoRequest<Response.TaskGroupResponse>($"{project}/_apis/distributedtask/taskgroups/{id}", new Dictionary<string, object>
        {
            { "api-version", "6.0-preview.1" }
        });
}
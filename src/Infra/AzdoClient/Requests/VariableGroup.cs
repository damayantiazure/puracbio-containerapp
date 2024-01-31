using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class VariableGroup
{
    public static AzdoRequest<Response.VariableGroup> VariableGroups(string projectId) => 
        new AzdoRequest<Response.VariableGroup>(
            $"{projectId}/_apis/distributedtask/variablegroups",
            new Dictionary<string, object> { ["api-version"] = "5.1-preview.1" });

    public static AzdoRequest<Response.VariableGroup> VariableGroupWithId(string projectId, int id) =>
        new AzdoRequest<Response.VariableGroup>(
            $"{projectId}/_apis/distributedtask/variablegroups/{id}",
            new Dictionary<string, object> { ["api-version"] = "5.1-preview.1" });
}
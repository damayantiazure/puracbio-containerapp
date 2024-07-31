using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class WorkItemTracking
{
    public static IAzdoRequest<QueryByWiql, WorkItemQueryResult> QueryByWiql(string project, int? top = null) =>
        new AzdoRequest<QueryByWiql, WorkItemQueryResult>(
            $"{project}/_apis/wit/wiql", new Dictionary<string, object>
            {
                ["api-version"] = "5.1",
                ["$top"] = top
            });

    public static IAzdoRequest<WorkItem> GetWorkItem(string project, int id, IEnumerable<string> fields = null,
        DateTime? asOf = null) => new AzdoRequest<WorkItem>(
        $"{project}/_apis/wit/workitems/{id}", new Dictionary<string, object>
        {
            ["api-version"] = "5.1",
            ["asOf"] = asOf,
            ["fields"] = fields == null ? null : string.Join(",", fields)
        });
}
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class TestManagement
{
    public static IEnumerableRequest<TestRun> QueryTestRuns(string project, DateTime mindate, DateTime maxdate, bool isAutomated) =>
        new AzdoRequest<Response.TestRun>($"{project}/_apis/test/runs",
            new Dictionary<string, object>
            {
                ["api-version"] = "5.0",
                ["isAutomated"] = isAutomated,
                ["minLastUpdatedDate"] = mindate,
                ["maxLastUpdatedDate"] = maxdate
            }).AsEnumerable();
}
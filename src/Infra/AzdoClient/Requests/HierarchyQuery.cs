using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class HierarchyQuery
{
    public static IAzdoRequest<object, JObject> ProjectInfo(string projectId)
    {
        return new AzdoRequest<object, JObject>(
            $"_apis/Contribution/HierarchyQuery/project/{projectId}",
            CommonApiVersion, 60);
    }

    public static object Approvals(string projectName, string runId, string stageId)
    {
        return 
            new
            {
                contributionIds = new[] { "ms.vss-build-web.checks-panel-data-provider" },
                dataProviderContext = new
                {
                    properties = new
                    {
                        buildId = runId,
                        stageIds = stageId,
                        checkListItemType = 1,
                        sourcePage = new
                        {
                            routeId = "ms.vss-build-web.ci-results-hub-route",
                            routeValues = new
                            {
                                project = projectName,
                                viewname = "build-results",
                                controller = "ContributedPage",
                                action = "Execute"
                            }
                        }
                    }
                }
            };
    }

    public static object PipelineVersion(string pipelineId, string sourceBranch, string projectName) 
    {
        return
            new
            {
                contributionIds = new[] { "ms.vss-build-web.pipeline-run-parameters-data-provider" },
                dataProviderContext = new
                {
                    properties = new
                    {
                        onlyFetchTemplateParameters = false,
                        pipelineId = pipelineId,
                        sourceBranch = sourceBranch,
                        sourcePage = new
                        {
                            routeValues = new
                            {
                                project = projectName
                            }
                        }
                    }
                }
            };
    }

    private static readonly IDictionary<string, object> CommonApiVersion = 
        new Dictionary<string, object>
        {
            {"api-version", "5.0-preview.1"}
        };
}
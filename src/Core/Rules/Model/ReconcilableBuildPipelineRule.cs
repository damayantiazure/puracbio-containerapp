using Rabobank.Compliancy.Infra.AzdoClient;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Model;

public abstract class ReconcilableBuildPipelineRule : BuildPipelineRule
{
    protected ReconcilableBuildPipelineRule(IAzdoRestClient azdoClient) : base(azdoClient)
    {
    }

    public abstract Task ReconcileAsync(string organization, string projectId, string itemId);

    public async Task<bool> ReconcileAndEvaluateAsync(string organization, string projectId, string itemId)
    {
        await ReconcileAsync(organization, projectId, itemId);
        var buildDefinition = await GetBuildDefinitionAsync(organization, projectId, itemId);
        return await EvaluateAsync(organization, projectId, buildDefinition);
    }
}
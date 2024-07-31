using Rabobank.Compliancy.Infra.AzdoClient;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Model;

public abstract class ReconcilableClassicReleasePipelineRule : ClassicReleasePipelineRule
{
    protected ReconcilableClassicReleasePipelineRule(IAzdoRestClient azdoClient) : base(azdoClient)
    {
    }

    public abstract Task ReconcileAsync(string organization, string projectId, string itemId);

    public async Task<bool> ReconcileAndEvaluateAsync(string organization, string projectId, string releaseDefinitionId)
    {
        await ReconcileAsync(organization, projectId, releaseDefinitionId);
        var buildDefinition = await GetReleaseDefinitionAsync(organization, projectId, releaseDefinitionId);
        return await EvaluateAsync(organization, projectId, buildDefinition);
    }
}
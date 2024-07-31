using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Model;

public abstract class ClassicReleasePipelineRule
{
    private readonly IAzdoRestClient _azdoClient;

    protected ClassicReleasePipelineRule(IAzdoRestClient azdoClient)
    {
        _azdoClient = azdoClient;
    }

    public async Task<bool> EvaluateAsync(Domain.Compliancy.Pipeline pipeline)
    {
        var projectId = pipeline.Project.Id.ToString();

        // Fetching the old "ReleaseDefinition" object, this should become redundant when the rest of the code will use this EvaluateAsync (with
        // a new pipeline object) instead of the bottom one. This can be applied after Pipeline is refactored to contain all information.
        var releaseDefinition = await GetReleaseDefinitionAsync(pipeline.Project.Organization, projectId, pipeline.Id.ToString());

        return await EvaluateAsync(pipeline.Project.Organization, projectId, releaseDefinition);
    }

    public abstract Task<bool> EvaluateAsync(string organization, string projectId, ReleaseDefinition releasePipeline);

    protected async Task<ReleaseDefinition> GetReleaseDefinitionAsync(string organization, string projectId, string pipelineId)
    {
        return await _azdoClient.GetAsync(ReleaseManagement.Definition(projectId, pipelineId), organization);
    }
}
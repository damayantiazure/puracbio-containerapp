using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Model;

public abstract class BuildPipelineRule
{
    private readonly IAzdoRestClient _azdoClient;

    protected BuildPipelineRule(IAzdoRestClient azdoClient)
    {
        _azdoClient = azdoClient;
    }

    public async Task<bool> EvaluateAsync(Domain.Compliancy.Pipeline pipeline)
    {
        var projectId = pipeline.Project.Id.ToString();

        // Fetching the old "BuildDefinition" object, this should become redundant when the rest of the code will use this EvaluateAsync (with
        // a new pipeline object) instead of the bottom one. This can be applied after Pipeline is refactored to contain all information.
        var buildDefinition = await GetBuildDefinitionAsync(pipeline.Project.Organization, projectId, pipeline.Id.ToString());
        return await EvaluateAsync(pipeline.Project.Organization, projectId, buildDefinition);
    }

    public abstract Task<bool> EvaluateAsync(string organization, string projectId, BuildDefinition buildPipeline);

    protected async Task<BuildDefinition> GetBuildDefinitionAsync(string organization, string projectId, string buildDefinitionId)
    {
        return await _azdoClient.GetAsync(Builds.BuildDefinition(projectId, buildDefinitionId), organization);
    }
}
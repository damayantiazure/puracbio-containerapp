using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Model;

public interface IBuildPipelineRule : IPipelineRule
{
    Task<bool> EvaluateAsync(string organization, string projectId, BuildDefinition buildPipeline);
}
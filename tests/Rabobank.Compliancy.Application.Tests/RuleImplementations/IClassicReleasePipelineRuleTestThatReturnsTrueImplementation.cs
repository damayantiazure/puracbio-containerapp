using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Application.Tests.RuleImplementations;

public class IClassicReleasePipelineRuleTestThatReturnsTrueImplementation : ClassicReleasePipelineRule, IClassicReleasePipelineRule, IReconcile
{
    private static readonly JsonSerializer serializer = new() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

    public IClassicReleasePipelineRuleTestThatReturnsTrueImplementation(IAzdoRestClient client) : base(client)
    {
    }

    public string Name => nameof(IClassicReleasePipelineRuleTestThatReturnsTrueImplementation);

    public string Description => nameof(IClassicReleasePipelineRuleTestThatReturnsTrueImplementation);

    public string Link => throw new NotImplementedException();

    public BluePrintPrinciple[] Principles => Array.Empty<BluePrintPrinciple>();

    public string[] Impact => Array.Empty<string>();

    public override Task<bool> EvaluateAsync(string organization, string projectId, ReleaseDefinition releasePipeline)
    {
        return Task.FromResult(true);
    }

    public Task<bool> ReconcileAndEvaluateAsync(string organization, string projectId, string itemId)
    {
        return Task.FromResult(true);
    }

    public Task ReconcileAsync(string organization, string projectId, string itemId)
    {
        return Task.CompletedTask;
    }
}
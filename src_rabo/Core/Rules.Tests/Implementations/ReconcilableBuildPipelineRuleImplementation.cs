using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Infra.AzdoClient;
using System;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Tests.Implementations;

public class ReconcilableBuildPipelineRuleImplementation : ReconcilableBuildPipelineRule, IReconcile
{
    private readonly bool _evaluationResult;

    public ReconcilableBuildPipelineRuleImplementation(IAzdoRestClient azdoClient, bool evaluationResult) : base(azdoClient)
    {
        _evaluationResult = evaluationResult;
    }

    public string Name => nameof(ReconcilableBuildPipelineRuleImplementation);

    public string[] Impact => Array.Empty<string>();

    public override Task<bool> EvaluateAsync(string organization, string projectId, Infra.AzdoClient.Response.BuildDefinition buildPipeline)
    {
        return Task.FromResult(_evaluationResult);
    }

    public override Task ReconcileAsync(string organization, string projectId, string itemId)
    {
        return Task.CompletedTask;
    }
}
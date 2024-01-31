using Rabobank.Compliancy.Core.Rules.Model;

namespace Rabobank.Compliancy.Application.Tests.RuleImplementations;

public class NobodyCanDeleteTheProjectTestThatReturnsTrueImplementation : IProjectRule, IProjectReconcile
{
    public string Name => nameof(NobodyCanDeleteTheProjectTestThatReturnsTrueImplementation);
    public string Description => nameof(NobodyCanDeleteTheProjectTestThatReturnsTrueImplementation);
    public string Link => throw new NotImplementedException();
    BluePrintPrinciple[] IRule.Principles => new[] { BluePrintPrinciples.Auditability };

    string[] IProjectReconcile.Impact => throw new NotImplementedException();

    public async Task<bool> EvaluateAsync(string organization, string projectId)
    {
        return await Task.FromResult(true);
    }

    public async Task<bool> EvaluateAsync(Domain.Compliancy.Project project)
    {
        return await Task.FromResult(true);
    }

    public async Task<bool> ReconcileAndEvaluateAsync(string organization, string projectId)
    {
        return await Task.FromResult(true);
    }

    public Task ReconcileAsync(string organization, string projectId)
    {
        return Task.CompletedTask;
    }
}
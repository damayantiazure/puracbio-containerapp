using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Tests.RuleImplementations;

public class NobodyCanDeleteTheRepositoryTestThatReturnsTrueImplementation : IRepositoryRule, IProjectReconcile
{
    public string Name => nameof(NobodyCanDeleteTheRepositoryTestThatReturnsTrueImplementation);
    public string Description => nameof(NobodyCanDeleteTheRepositoryTestThatReturnsTrueImplementation);
    public string Link => throw new NotImplementedException();
    BluePrintPrinciple[] IRule.Principles => new[] { BluePrintPrinciples.Auditability };

    string[] IProjectReconcile.Impact => throw new NotImplementedException();

    public async Task<bool> EvaluateAsync(string organization, string projectId, string repositoryId)
    {
        return await Task.FromResult(true);
    }

    public async Task<bool> EvaluateAsync(GitRepo gitRepo)
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
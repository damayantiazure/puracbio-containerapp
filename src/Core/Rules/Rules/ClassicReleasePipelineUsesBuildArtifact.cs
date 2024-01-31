using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public class ClassicReleasePipelineUsesBuildArtifact : ClassicReleasePipelineRule, IClassicReleasePipelineRule
{
    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.ClassicReleasePipelineUsesBuildArtifact;
    [ExcludeFromCodeCoverage]
    string IRule.Description => "Classic release pipeline uses build artifacts";
    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/aY8AD";
    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles =>
        new[] { BluePrintPrinciples.CodeIntegrity };

    public ClassicReleasePipelineUsesBuildArtifact(IAzdoRestClient client) : base(client)
    {
    }

    public override Task<bool> EvaluateAsync(
        string organization, string projectId, ReleaseDefinition releasePipeline)
    {
        if (releasePipeline == null)
        {
            throw new ArgumentNullException(nameof(releasePipeline));
        }

        var result = releasePipeline.Artifacts.Any() &&
                     releasePipeline.Artifacts.All(a => a.Type == "Build");

        return Task.FromResult(result);
    }
}
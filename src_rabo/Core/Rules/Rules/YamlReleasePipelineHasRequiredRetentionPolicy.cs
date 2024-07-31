using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Response = Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public class YamlReleasePipelineHasRequiredRetentionPolicy : ReconcilableBuildPipelineRule, IYamlReleasePipelineRule, IReconcile
{
    private readonly IAzdoRestClient _client;
    private const int RequiredRetentionDays = 450;

    public YamlReleasePipelineHasRequiredRetentionPolicy(IAzdoRestClient client)
        : base(client)
        => _client = client;

    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.YamlReleasePipelineHasRequiredRetentionPolicy;
    [ExcludeFromCodeCoverage]
    string IReconcile.Name => RuleNames.YamlReleasePipelineHasRequiredRetentionPolicy;
    [ExcludeFromCodeCoverage]
    string IRule.Description =>
        "All pipeline runs are retained";
    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/sDSgDw";
    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles =>
        new[] { BluePrintPrinciples.Auditability };

    [ExcludeFromCodeCoverage]
    string[] IReconcile.Impact => new[] {
        "In project settings the retention policy for days to keep runs is set to 450 days."
    };

    public async override Task<bool> EvaluateAsync(
        string organization, string projectId, Response.BuildDefinition buildPipeline)
    {
        ValidateInput(organization, projectId);

        return await HasCorrectRetentionSettings(organization, projectId);
    }

    public async override Task ReconcileAsync(string organization, string projectId, string itemId)
    {
        ValidateInput(organization, projectId);

        if (await HasCorrectRetentionSettings(organization, projectId))
        {
            return;
        }

        await _client.PatchAsync(Builds.SetRetention(projectId), CreateSetRetentionBody(), organization);
    }

    private static void ValidateInput(string organization, string projectId)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }

        if (projectId == null)
        {
            throw new ArgumentNullException(nameof(projectId));
        }
    }

    private async Task<bool> HasCorrectRetentionSettings(string organization, string projectId)
    {
        var retentionSettings = await _client.GetAsync(Builds.Retention(projectId), organization);
        return retentionSettings?.PurgeRuns != null &&
               retentionSettings.PurgeRuns.Value >= RequiredRetentionDays;
    }

    private static Response.SetRetention CreateSetRetentionBody() =>
        new Response.SetRetention()
        {
            RunRetention = new Response.RunRetention()
            {
                Value = RequiredRetentionDays
            }
        };
}
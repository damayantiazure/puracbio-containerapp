using Flurl.Http;
using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Core.Rules.Exceptions;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Requests = Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public class ClassicReleasePipelineHasRequiredRetentionPolicy : ReconcilableClassicReleasePipelineRule, IClassicReleasePipelineRule, IReconcile
{
    private readonly IAzdoRestClient _client;
    private readonly int RequiredRetentionDays = 450;

    public ClassicReleasePipelineHasRequiredRetentionPolicy(IAzdoRestClient client) : base(client) => _client = client;
    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.ClassicReleasePipelineHasRequiredRetentionPolicy;
    [ExcludeFromCodeCoverage]
    string IReconcile.Name => RuleNames.ClassicReleasePipelineHasRequiredRetentionPolicy;
    [ExcludeFromCodeCoverage]
    string IRule.Description => "All releases are retained";
    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/9o8AD";
    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles =>
        new[] { BluePrintPrinciples.Auditability };

    [ExcludeFromCodeCoverage]
    string[] IReconcile.Impact => new[] {
        "In project settings the maximum retention policy is set to 450 days.",
        "On the pipeline the days to retain a release is set to 450 days for every stage.",
        "On the pipeline the checkbox to retain associated artifacts is enabled for every stage."
    };

    public override Task<bool> EvaluateAsync(string organization, string projectId, ReleaseDefinition releasePipeline)
    {
        if (releasePipeline == null)
        {
            throw new ArgumentNullException(nameof(releasePipeline));
        }

        var result = releasePipeline
            .Environments
            .Select(e => e.RetentionPolicy)
            .All(r => r.DaysToKeep >= RequiredRetentionDays && r.RetainBuild);

        return Task.FromResult(result);
    }

    public async override Task ReconcileAsync(string organization, string projectId, string itemId)
    {
        var releaseSettings = await _client.GetAsync(Requests.ReleaseManagement.Settings(projectId),
            organization);
        if (!HasRequiredReleaseSettings(releaseSettings))
        {
            await _client.PutAsync(Requests.ReleaseManagement.Settings(projectId),
                UpdateReleaseSettings(releaseSettings), organization);
        }

        var releasePipeline = await _client.GetAsync(
            new VsrmRequest<object>($"{projectId}/_apis/release/definitions/{itemId}").AsJson(),
            organization);

        try
        {
            await _client.PutAsync(
                new VsrmRequest<object>($"{projectId}/_apis/release/definitions/{itemId}",
                    new Dictionary<string, object> { { "api-version", "5.0" } }),
                UpdateReleaseDefinition(releasePipeline), organization);
        }
        catch (FlurlHttpException e)
        {
            throw new InvalidClassicPipelineException(ErrorMessages.InvalidClassicPipeline(e.Message));
        }
    }

    private bool HasRequiredReleaseSettings(ReleaseSettings settings) =>
        settings.RetentionSettings.MaximumEnvironmentRetentionPolicy.DaysToKeep >= RequiredRetentionDays;

    private ReleaseSettings UpdateReleaseSettings(ReleaseSettings settings)
    {
        if (settings.RetentionSettings.MaximumEnvironmentRetentionPolicy.DaysToKeep < RequiredRetentionDays)
        {
            settings.RetentionSettings.MaximumEnvironmentRetentionPolicy.DaysToKeep = RequiredRetentionDays;
        }

        settings.RetentionSettings.MaximumEnvironmentRetentionPolicy.RetainBuild = true;
        return settings;
    }

    private JToken UpdateReleaseDefinition(JToken pipeline)
    {
        pipeline
            .SelectTokens("environments[*].retentionPolicy.daysToKeep")
            .ToList()
            .ForEach(t => t.Replace(RequiredRetentionDays));
        pipeline
            .SelectTokens("environments[*].retentionPolicy.retainBuild")
            .ToList()
            .ForEach(t => t.Replace(true));

        return pipeline;
    }
}
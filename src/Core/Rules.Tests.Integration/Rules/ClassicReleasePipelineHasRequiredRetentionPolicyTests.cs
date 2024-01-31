using Task = System.Threading.Tasks.Task;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using Xunit;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;

namespace Rabobank.Compliancy.Core.Rules.Tests.Integration.Rules;

public class ClassicReleasePipelineHasRequiredRetentionPolicyTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    private const string PipelineId = "1";
    private const int IncompliantRetention = 30;

    public ClassicReleasePipelineHasRequiredRetentionPolicyTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Token);
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task EvaluateAndReconcileIntegrationTest()
    {
        await _client.PutAsync(new VsrmRequest<object>($"{_config.Project}/_apis/release/definitions/{PipelineId}",
                new Dictionary<string, object> { { "api-version", "5.0" } }), await IncompliantReleaseRetentionAsync(),
            _config.Organization);
        await _client.PutAsync(ReleaseManagement.Settings(_config.Project), IncompliantMaximumRetention(),
            _config.Organization);

        var releasePipelineBefore = await _client.GetAsync(ReleaseManagement.Definition(_config.Project, PipelineId),
            _config.Organization);
        var resultBefore = await new ClassicReleasePipelineHasRequiredRetentionPolicy(_client)
            .EvaluateAsync(_config.Organization, _config.Project, releasePipelineBefore);
        resultBefore.ShouldBe(false);

        await new ClassicReleasePipelineHasRequiredRetentionPolicy(_client).ReconcileAsync(_config.Organization,
            _config.Project, PipelineId);

        var releasePipelineAfter = await _client.GetAsync(ReleaseManagement.Definition(_config.Project, PipelineId), 
            _config.Organization);
        var resultAfter = await new ClassicReleasePipelineHasRequiredRetentionPolicy(_client)
            .EvaluateAsync(_config.Organization, _config.Project, releasePipelineAfter);
        resultAfter.ShouldBe(true);
    }

    private async Task<JToken> IncompliantReleaseRetentionAsync()
    {
        var pipeline = await _client.GetAsync(
            new VsrmRequest<object>($"{_config.Project}/_apis/release/definitions/{PipelineId}")
                .AsJson(), _config.Organization);

        pipeline
            .SelectTokens("environments[*].retentionPolicy.daysToKeep")
            .ToList()
            .ForEach(t => t.Replace(IncompliantRetention));
        pipeline
            .SelectTokens("environments[*].retentionPolicy.retainBuild")
            .ToList()
            .ForEach(t => t.Replace(false));

        return pipeline;
    }

    private static ReleaseSettings IncompliantMaximumRetention() => 
        new ReleaseSettings
        {
            ComplianceSettings = new ComplianceSettings
            {
                CheckForCredentialsAndOtherSecrets = true
            },
            RetentionSettings = new RetentionSettings
            {
                DaysToKeepDeletedReleases = 14,
                DefaultEnvironmentRetentionPolicy = new RetentionPolicy
                {
                    DaysToKeep = 30,
                    ReleasesToKeep = 25,
                    RetainBuild = true
                },
                MaximumEnvironmentRetentionPolicy = new RetentionPolicy
                {
                    DaysToKeep = IncompliantRetention,
                    ReleasesToKeep = 100,
                    RetainBuild = true
                }
            }
        };
}
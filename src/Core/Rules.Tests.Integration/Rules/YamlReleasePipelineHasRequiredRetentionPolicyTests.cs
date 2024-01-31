using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Response = Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Core.Rules.Tests.Integration.Rules;

public class YamlReleasePipelineHasRequiredRetentionPolicyTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    private const int IncompliantRetention = 100;

    public YamlReleasePipelineHasRequiredRetentionPolicyTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Token);
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task EvaluateAndReconcileIntegrationTest()
    {
        var rule = new YamlReleasePipelineHasRequiredRetentionPolicy(_client);

        var retentionSettingBefore = await _client.PatchAsync(Builds.SetRetention(_config.Project), 
            IncompliantRetentionBody(), _config.Organization);
        retentionSettingBefore.PurgeRuns.Value.ShouldBe(IncompliantRetention);

        Thread.Sleep(2000);
        var resultBefore = await rule.EvaluateAsync(_config.Organization, _config.Project, null);
        resultBefore.ShouldBe(false);

        await rule.ReconcileAsync(_config.Organization, _config.Project, null);

        Thread.Sleep(2000);
        var resultAfter = await rule.EvaluateAsync(_config.Organization, _config.Project, null);
        resultAfter.ShouldBe(true);
    }

    private static Response.SetRetention IncompliantRetentionBody() =>
        new Response.SetRetention()
        {
            RunRetention = new Response.RunRetention()
            {
                Value = IncompliantRetention
            }
        };
}
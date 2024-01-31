using System;
using System.Threading.Tasks;
using MemoryCache.Testing.Moq;
using Polly;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using Xunit;

namespace Rabobank.Compliancy.Core.Rules.Tests.Integration.Rules;

public class NobodyCanDeleteBuildsTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public NobodyCanDeleteBuildsTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Token);
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task ReconcileIntegrationTest()
    {
        var buildPipeline = await _client.GetAsync(Builds.BuildDefinition(_config.Project, "505"),
            _config.Organization);
        var cache = Create.MockedMemoryCache();

        await ManagePermissions
            .SetSecurityContextToSpecificBuildPipeline(_client, cache, _config.Organization, _config.Project, buildPipeline.Id, buildPipeline.Path)
            .SetPermissionsToBeInScope(BuildDefinitionBits.DeleteBuilds)
            .UpdatePermissionsInScopeForGroupsInScopeAsync(PermissionLevelId.Allow);

        var rule = new NobodyCanDeleteBuilds(_client, cache);
        (await rule.EvaluateAsync(_config.Organization, _config.Project, buildPipeline))
            .ShouldBe(false);

        await rule.ReconcileAsync(_config.Organization, _config.Project, buildPipeline.Id);
        await Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(Constants.NumRetries, t => TimeSpan.FromSeconds(1))
            .ExecuteAsync(async () =>
            {
                (await rule.EvaluateAsync(_config.Organization, _config.ProjectId, buildPipeline)).ShouldBe(true);
            });
    }
}
using MemoryCache.Testing.Moq;
using Polly;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Shouldly;
using System;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Integration.Rules;

public class NobodyCanDeleteTheProjectTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public NobodyCanDeleteTheProjectTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Token);
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task ReconcileIntegrationTest()
    {
        var cache = Create.MockedMemoryCache();

        await ManagePermissions
            .SetSecurityContextToTeamProject(_client, cache, _config.Organization, _config.Project)
            .SetPermissionsToBeInScope((ProjectBits.DeleteProject, SecurityNamespaceIds.Project))
            .UpdatePermissionsInScopeForGroupsInScopeAsync(PermissionLevelId.Allow);

        var rule = new NobodyCanDeleteTheProject(_client, cache);
        (await rule.EvaluateAsync(_config.Organization, _config.Project))
            .ShouldBe(false);

        await rule.ReconcileAsync(_config.Organization, _config.Project);
        await Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(Constants.NumRetries, t => TimeSpan.FromSeconds(t))
            .ExecuteAsync(async () =>
            {
                (await rule.EvaluateAsync(_config.Organization, _config.Project)).ShouldBe(true);
            });
    }
}
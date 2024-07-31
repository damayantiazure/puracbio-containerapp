using MemoryCache.Testing.Moq;
using Polly;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;
using Permissions = Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits.RepositoryBits;

namespace Rabobank.Compliancy.Core.Rules.Tests.Integration.Rules;

public class NobodyCanDeleteTheRepositoryTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public NobodyCanDeleteTheRepositoryTests(TestConfig config)
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
            .SetSecurityContextToSpecificRepository(_client, cache, _config.Organization, _config.Project, _config.RepositoryId)
            .SetPermissionsToBeInScope(Permissions.DeleteRepository)
            .UpdatePermissionsInScopeForGroupsInScopeAsync(PermissionLevelId.Allow);

        var rule = new NobodyCanDeleteTheRepository(_client, cache);
        (await rule.EvaluateAsync(_config.Organization, _config.Project, _config.RepositoryId))
            .ShouldBe(false);

        await rule.ReconcileAsync(_config.Organization, _config.Project, _config.RepositoryId);
        await Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(Constants.NumRetries, t => TimeSpan.FromSeconds(t))
            .ExecuteAsync(async () =>
            {
                (await rule.EvaluateAsync(_config.Organization, _config.ProjectId, _config.RepositoryId)).ShouldBe(true);
            });
    }
}
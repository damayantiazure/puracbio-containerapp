using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Rabobank.Compliancy.Core.Rules.Helpers;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.StorageClient;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Bits = Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;

namespace Rabobank.Compliancy.Core.Rules.Tests.Integration.Rules;

public class NobodyCanManageEnvironmentGatesAndDeployTests : IClassFixture<TestConfig>
{
    private const string PipelineId = "507";
    private const int EnvironmentId = 110;
    private const string AdministratorRole = "Administrator";

    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;
    private readonly IMemoryCache _cache;

    public NobodyCanManageEnvironmentGatesAndDeployTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Token);
        _cache = Create.MockedMemoryCache();
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task EvaluateAndReconcileIntegrationTest()
    {
        //Set permissions to incompliant
        var buildPipeline = await _client.GetAsync(Builds.BuildDefinition(_config.Project, PipelineId),
            _config.Organization);
        var groups = await _client.GetAsync(ApplicationGroup.ApplicationGroups(_config.Project), _config.Organization);
        var paGroupId = groups.Identities
            .First(g => g.FriendlyDisplayName == AzureDevOpsGroups.ProjectAdministrators)
            .TeamFoundationId;

        var request = Environments.UpdateSecurity(_config.Project, EnvironmentId);
        var body = Environments.CreateUpdateSecurityBody(paGroupId, AdministratorRole);

        await _client.PutAsync(request, body, _config.Organization);

        await ManagePermissions
            .SetSecurityContextToSpecificBuildPipeline(_client, _cache, _config.Organization, _config.Project, buildPipeline.Id, buildPipeline.Path)
            .SetApplicationGroupsInScopeByDisplayName(AzureDevOpsGroups.ProjectAdministrators, AzureDevOpsGroups.ProductionEnvironmentOwners)
            .SetPermissionsToBeInScope(Bits.BuildDefinitionBits.QueueBuilds)
            .SetPermissionLevelIdsThatAreOkToHave(PermissionLevelId.Allow, PermissionLevelId.AllowInherited)
            .UpdatePermissionsInScopeForGroupsInScopeAsync(PermissionLevelId.Allow);

        await ManagePermissions
            .SetSecurityContextToSpecificRepository(_client, _cache, _config.Organization, _config.Project, buildPipeline.Repository.Id)
            .SetApplicationGroupsInScopeByDisplayName(AzureDevOpsGroups.ProjectAdministrators, AzureDevOpsGroups.ProductionEnvironmentOwners)
            .SetPermissionsToBeInScope(Bits.RepositoryBits.Contribute, Bits.RepositoryBits.ForcePush)
            .SetPermissionLevelIdsThatAreOkToHave(PermissionLevelId.Allow, PermissionLevelId.AllowInherited)
            .UpdatePermissionsInScopeForGroupsInScopeAsync(PermissionLevelId.Allow);

        //Arrange
        var resolverMock = new Mock<IPipelineRegistrationResolver>();
        resolverMock.Setup(m => m.ResolveProductionStagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new[] { "Prod" });
        var helper = new YamlEnvironmentHelper(_client, resolverMock.Object);

        //Act & Assert 1 - Evaluate before
        var rule = new NobodyCanManageEnvironmentGatesAndDeploy(_client, _cache, helper);
        var resultBefore = await rule.EvaluateAsync(_config.Organization, _config.Project, buildPipeline);
        resultBefore.ShouldBeFalse();

        //Act & Assert 2 - Reconcile and Evaluate after
        await rule.ReconcileAsync(_config.Organization, _config.Project, buildPipeline.Id);
        var resultAfter = await rule.EvaluateAsync(_config.Organization, _config.Project, buildPipeline);
        resultAfter.ShouldBeTrue();
    }
}
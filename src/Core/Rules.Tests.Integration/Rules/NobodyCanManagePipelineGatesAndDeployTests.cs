using MemoryCache.Testing.Moq;
using NSubstitute;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.StorageClient;
using Shouldly;
using Xunit;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Integration.Rules;

public class NobodyCanManagePipelineGatesAndDeployTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;
    private static readonly string _contributorGroup = AzureDevOpsGroups.Contributors;
    private static readonly string _peoGroup = AzureDevOpsGroups.ProductionEnvironmentOwners;

    private const string PipelineId = "1";
    private const string StageId = "1";

    public NobodyCanManagePipelineGatesAndDeployTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Token);
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task EvaluateAndReconcileIntegrationTest()
    {
        var releasePipeline = await _client.GetAsync(ReleaseManagement.Definition(_config.Project, PipelineId),
            _config.Organization);

        await ManagePermissions
            .SetSecurityContextToSpecificReleasePipeline(_client, Create.MockedMemoryCache(), _config.Organization, _config.Project,
                releasePipeline.Id, releasePipeline.Path)
            .SetApplicationGroupsInScopeByDisplayName(new[] { _contributorGroup, _peoGroup })
            .SetPermissionsToBeInScope(ReleaseDefinitionBits.CreateReleases)
            .UpdatePermissionsInScopeForGroupsInScopeAsync(PermissionLevelId.Allow);

        await ManagePermissions
            .SetSecurityContextToReleasePipelineStage(_client, Create.MockedMemoryCache(), _config.Organization, _config.Project,
                releasePipeline.Id, StageId, releasePipeline.Path)
            .SetApplicationGroupsInScopeByDisplayName(new[] { _contributorGroup, _peoGroup })
            .SetPermissionsToBeInScope(ReleasePipelineStageBits.ManageApprovals, ReleasePipelineStageBits.ManageDeployments)
            .UpdatePermissionsInScopeForGroupsInScopeAsync(PermissionLevelId.Allow);

        var productionItemsResolver = Substitute.For<IPipelineRegistrationResolver>();
        productionItemsResolver.ResolveProductionStagesAsync(_config.Organization, _config.Project, releasePipeline.Id).Returns(new[] { StageId });

        var evaluate1 = new NobodyCanManagePipelineGatesAndDeploy(_client, Create.MockedMemoryCache(), productionItemsResolver);
        (await evaluate1.EvaluateAsync(_config.Organization, _config.Project, releasePipeline)).ShouldBe(false);

        await Task.Delay(3000);
        var reconcile = new NobodyCanManagePipelineGatesAndDeploy(_client, Create.MockedMemoryCache(), productionItemsResolver);
        await reconcile.ReconcileAsync(_config.Organization, _config.Project, releasePipeline.Id);

        await Task.Delay(3000);
        var evaluate2 = new NobodyCanManagePipelineGatesAndDeploy(_client, Create.MockedMemoryCache(), productionItemsResolver);
        (await evaluate2.EvaluateAsync(_config.Organization, _config.Project, releasePipeline)).ShouldBe(true);
    }
}
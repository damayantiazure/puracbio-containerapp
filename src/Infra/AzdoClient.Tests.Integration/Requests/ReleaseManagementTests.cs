using Flurl.Http.Testing;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Environment = Rabobank.Compliancy.Infra.AzdoClient.Response.Environment;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

[Trait("category", "integration")]
public class ReleaseManagementTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;
    private readonly string _project;

    public ReleaseManagementTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(config.Organization, config.Token);
        _project = config.ProjectName;
    }


    [Fact]
    public async Task ReleaseWithApproval()
    {
        const int id = 79;

        var response = File.ReadAllText(Path.Join("Assets", "Approved.json"));

        var request = new AzdoRequest<Response.Release>("/keeas");

        using var httpTest = new HttpTest();
        httpTest.RespondWith(status: 200, body: response);
        var client = new AzdoRestClient("dummy", "pat");
        var release = await client.GetAsync(request);
        release.Id.ShouldBe(id);
        release.Environments.ShouldNotBeEmpty();
        release.Tags.ShouldNotBeEmpty();
        var env = release.Environments.Skip(1).First();
        env.Id.ShouldNotBe(0);
        env.PreDeployApprovals.ShouldNotBeEmpty();
        env.DeploySteps.ShouldNotBeEmpty();
        env.Name.ShouldNotBeNullOrEmpty();
        env.DeployPhasesSnapshot.ShouldNotBeEmpty();

        var phaseSnapshot = env.DeployPhasesSnapshot.First();
        phaseSnapshot.PhaseType.ShouldNotBeEmpty();
        phaseSnapshot.DeploymentInput.ShouldNotBeNull();
        phaseSnapshot.DeploymentInput.QueueId.ShouldNotBe(0);

        var deploy = env.DeploySteps.First();
        deploy.RequestedFor.ShouldNotBeNull();
        deploy.LastModifiedBy.ShouldNotBeNull();

        var predeploy = env.PreDeployApprovals.First();
        predeploy.Status.ShouldNotBeNullOrEmpty();
        predeploy.ApprovalType.ShouldNotBeNullOrEmpty();
        predeploy.IsAutomated.ShouldBe(false);
        predeploy.ApprovedBy.ShouldNotBeNull();
        predeploy.ApprovedBy.DisplayName.ShouldNotBeNullOrEmpty();

        var conditions = env.Conditions.ToList();
        conditions.ShouldNotBeEmpty();

        var condition = conditions.First();
        condition.Result.ShouldBe(false);
        condition.Name.ShouldNotBeNullOrEmpty();
        condition.ConditionType.ShouldNotBeEmpty();
        condition.Value.ShouldNotBeNull();

        var artifact = release.Artifacts.First();
        artifact.Type.ShouldNotBeNull();
        artifact.Alias.ShouldNotBeNull();
        artifact.DefinitionReference?.Branch?.Name?.ShouldNotBeNullOrEmpty();
        artifact.DefinitionReference.Version.Id.ShouldNotBeNullOrEmpty();
        artifact.DefinitionReference.Version.Name.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task QueryReleasesByPipeline()
    {
        var releases = await _client.GetAsync(ReleaseManagement.Releases(
            _project, _config.ReleaseDefinitionId, "environments", "1-1-2019"));

        releases.ShouldNotBeNull();
        releases.ShouldNotBeEmpty();
        releases.SelectMany(r => r.Environments).ShouldNotBeNull();
        releases.SelectMany(r => r.Environments).ShouldNotBeEmpty();
    }

    [Fact]
    public async Task RequestForMultipleContinuesUsingContinuationToken()
    {
        var releases = await _client.GetAsync(
            new VsrmRequest<Response.Release>($"{_config.ProjectName}/_apis/release/releases/",
                new Dictionary<string, object>
                {
                    {"$top", "2"}
                }).AsEnumerable());
        releases.Count().ShouldBeGreaterThan(2);
    }

    [Fact]
    public async Task ConditionResultOnReleaseEnvironmentMustBeNullable()
    {
        var response = File.ReadAllText(Path.Join("Assets", "ConditionResultNull.json"));

        var request = new AzdoRequest<Environment>("/keeas");

        using var httpTest = new HttpTest();
        httpTest.RespondWith(status: 200, body: response);
        var client = new AzdoRestClient("dummy", "pat");
        await client.GetAsync(request);
    }

    [Fact]
    public async Task QueryReleaseApprovals()
    {
        var approvals = await _client.GetAsync(ReleaseManagement.Approvals(
            _project, _config.ReleaseId, "approved"));

        approvals.ShouldNotBeEmpty();
        approvals.Any(a => !a.IsAutomated);

        var approval = approvals.First(a => !a.IsAutomated);

        approval.ApprovalType.ShouldNotBeNull();
        approval.ApprovedBy.ShouldNotBeNull();
        approval.ApprovedBy.DisplayName.ShouldNotBeNull();
        approval.ApprovedBy.UniqueName.ShouldNotBeNull();
        approval.ApprovedBy.ShouldNotBeNull();
        approval.Status.ShouldBe("approved");
    }

    [Fact]
    public async Task QueryReleaseDefinitions()
    {
        var definitions = await _client.GetAsync(ReleaseManagement.Definitions(_config.ProjectName));
        definitions.ShouldAllBe(_ => !string.IsNullOrEmpty(_.Name));
        definitions.First().Links.ShouldNotBeNull();
    }

    [Fact]
    public async Task QueryReleaseDefinitionsWithExpandedEnvironments()
    {
        var definitions = await _client.GetAsync(ReleaseManagement.Definitions(_config.ProjectName, "environments"));
        definitions.ShouldContain(_ => _.Environments.Any());
    }

    [Fact]
    public async Task QueryReleaseDefinitionDetails()
    {
        var definition = await _client.GetAsync(ReleaseManagement.Definition(_config.ProjectName, _config.ReleaseDefinitionId));
        definition.Name.ShouldNotBeNull();
        definition.Links.ShouldNotBeNull();
        definition.Environments.ShouldNotBeEmpty();
        definition.Artifacts.ShouldNotBeEmpty();
        definition.CreatedBy.ShouldNotBeNull();
        definition.CreatedBy.UniqueName.ShouldNotBeNull();
        definition.ModifiedBy.ShouldNotBeNull();
        definition.ModifiedBy.UniqueName.ShouldNotBeNull();

        var environment = definition.Environments.First();
        environment.Name.ShouldNotBeEmpty();
        environment.DeployPhases.ShouldNotBeEmpty();

        var phase = environment.DeployPhases.First();
        phase.WorkflowTasks.ShouldNotBeEmpty();

        var task = phase.WorkflowTasks.First();
        task.Name.ShouldNotBeEmpty();

        var artifact = definition.Artifacts.First();
        artifact.Type.ShouldNotBeEmpty();
        artifact.DefinitionReference.Definition.Id.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetReleaseDefinitionDetails_WithRevision()
    {
        var definition = await _client.GetAsync(ReleaseManagement.Definition(_config.ProjectName, _config.ReleaseDefinitionId,
            _config.ReleasePipelineRevision), _config.Organization);

        definition.Name.ShouldNotBeNull();
        definition.Links.ShouldNotBeNull();
        definition.Environments.ShouldNotBeEmpty();
        definition.Artifacts.ShouldNotBeEmpty();
        definition.CreatedBy.ShouldNotBeNull();
        definition.CreatedBy.UniqueName.ShouldNotBeNull();
        definition.ModifiedBy.ShouldNotBeNull();
        definition.ModifiedBy.UniqueName.ShouldNotBeNull();

        var environment = definition.Environments.First();
        environment.Name.ShouldNotBeEmpty();
        environment.DeployPhases.ShouldNotBeEmpty();

        var phase = environment.DeployPhases.First();
        phase.WorkflowTasks.ShouldNotBeEmpty();

        var task = phase.WorkflowTasks.First();
        task.Name.ShouldNotBeEmpty();

        var artifact = definition.Artifacts.First();
        artifact.Type.ShouldNotBeEmpty();
        artifact.DefinitionReference.Definition.Id.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task CheckForCredentialsAndOtherSecrets()
    {
        var settings = await _client.GetAsync(ReleaseManagement.Settings(_project));
        settings.ComplianceSettings.CheckForCredentialsAndOtherSecrets.ShouldBeTrue();
    }

    [Fact()]
    [Trait("category", "integration")]
    public async Task QueryRelease()
    {
        var release = await _client.GetAsync(ReleaseManagement.Release(_config.ProjectName, _config.ReleaseId));
        release.ShouldNotBeNull();
        release.Id.ShouldNotBe(0);
        release.Tags.ShouldNotBeEmpty();
        release.Tags.Count().ShouldBe(2);
        release.Tags.First().ShouldBe("C000767033 [8fd01d22]");
        release.Links.ShouldNotBeNull();
        release.CreatedBy?.UniqueName.ShouldNotBeNull();
        release.CreatedFor?.UniqueName.ShouldNotBeNull();
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task AddAndRemoveTag()
    {
        const string tag = "Test tag";

        var tagsBefore = await _client.GetAsync(ReleaseManagement.Tags(_config.ProjectId, _config.ReleaseId));
        tagsBefore.Value.Contains(tag).ShouldBeFalse();

        var tagsAfterAdding = await _client.PatchAsync(ReleaseManagement.Tag(_config.ProjectId, _config.ReleaseId, tag), null);
        tagsAfterAdding.Value.Contains(tag).ShouldBeTrue();

        await _client.DeleteAsync(ReleaseManagement.Tag(_config.ProjectId, _config.ReleaseId, tag));
        var tagsAfterRemoving = await _client.GetAsync(ReleaseManagement.Tags(_config.ProjectId, _config.ReleaseId));
        tagsAfterRemoving.Value.Contains(tag).ShouldBeFalse();
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task TaskLogs_ShouldGetLogsForRelease()
    {
        // Arrange

        // Act
        var result = await _client.GetAsStringAsync(ReleaseManagement.TaskLogs(_config.ProjectId, 1, 1, 1, "5"));

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain("PowerShell Script");
        result.ShouldContain("Production deployment");
    }
}
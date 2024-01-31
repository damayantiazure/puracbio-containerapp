using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System;
using System.Linq;
using Xunit;
using Project = Rabobank.Compliancy.Infra.AzdoClient.Requests.Project;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

public class BuildsTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public BuildsTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Organization, _config.Token);
    }

    [Fact()]
    [Trait("category", "integration")]
    public async Task QueryArtifacts()
    {
        var artifacts = (await _client.GetAsync(Builds.Artifacts(_config.ProjectName, "136390")));
        artifacts.ShouldNotBeEmpty();

        var artifact = artifacts.First();
        artifact.Id.ShouldNotBe(0);

        artifact.Resource.ShouldNotBeNull();
        artifact.Resource.Type.ShouldBe("PipelineArtifact");
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task QueryChanges()
    {
        var changes = await _client.GetAsync(Builds.Changes(_config.ProjectName, _config.BuildId));
        changes.ShouldNotBeEmpty();

        var change = changes.First();
        change.Id.ShouldNotBeNull();
        change.Type.ShouldBe("TfsGit");
        change.Location.ShouldNotBeNull();
        change.Message.ShouldNotBeNull();
        change.Pusher.ShouldNotBeNull();
        change.Timestamp.ShouldNotBeNull();
        change.Author.ShouldNotBeNull();
        change.Author.UniqueName.ShouldNotBeNull();
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task QueryBuild()
    {
        var build = await _client.GetAsync(Builds.Build(_config.ProjectName, _config.BuildId));
        build.ShouldNotBeNull();
        build.Id.ShouldNotBe(0);
        build.Definition.ShouldNotBeNull();
        build.Definition.Revision.ShouldNotBeNull();
        build.Project.ShouldNotBeNull();
        build.Result.ShouldNotBeNull();
        build.SourceVersion.ShouldNotBeNull();
        build.Repository.Id.ShouldNotBeNull();
        build.Tags.ShouldNotBeEmpty();
        build.Tags.Count().ShouldBe(1);
        build.Tags.First().ShouldBe("IntegrationTest");
        build.Links.ShouldNotBeNull();
        build.RequestedFor.ShouldNotBeNull();
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task QueryLongRunningBuilds()
    {
        var queryOrder = "startTimeAscending";
        var minTime = DateTime.UtcNow.AddHours(-6).ToString("O");
        var build = await _client.GetAsync(Builds.LongRunningBuilds(_config.ProjectName, queryOrder, minTime));
        build.ToList().ShouldNotBeNull();
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task QueryBuildDefinition()
    {
        var buildDefinition =
            await _client.GetAsync(Builds.BuildDefinition(_config.ProjectName, _config.BuildDefinitionIdClassic));

        buildDefinition.ShouldNotBeNull();
        buildDefinition.Id.ShouldNotBeNull();
        buildDefinition.Name.ShouldNotBeNull();
        buildDefinition.Project.ShouldNotBeNull();
        buildDefinition.Process.Phases.First().Steps.First().Task.Id.ShouldNotBeNull();
        buildDefinition.Repository.ShouldNotBeNull();
        buildDefinition.Repository.Url.ShouldNotBeNull();
        buildDefinition.Links.ShouldNotBeNull();
        buildDefinition.AuthoredBy.UniqueName.ShouldNotBeNull();
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task GetYamlBuildPipelineWithRevision()
    {
        var buildPipeline = await _client.GetAsync(Builds.BuildDefinition(
                _config.ProjectName, _config.BuildDefinitionIdYaml, _config.BuildPipelineRevision),
            _config.Organization);

        buildPipeline.ShouldNotBeNull();
        buildPipeline.Id.ShouldNotBeNull();
        buildPipeline.Name.ShouldNotBeNull();
        buildPipeline.Project.ShouldNotBeNull();
        buildPipeline.Repository.ShouldNotBeNull();
        buildPipeline.Repository.Url.ShouldNotBeNull();
        buildPipeline.Links.ShouldNotBeNull();
    }

    [Fact]
    public async Task QueryBuildDefinitionsReturnsBuildDefinitionsWithTeamProjectReference()
    {
        var projectId = (await _client.GetAsync(Project.Properties(_config.ProjectName))).Id;

        var buildDefinitions = await _client.GetAsync(Builds.BuildDefinitions(projectId));
        buildDefinitions.ToList();

        buildDefinitions.ShouldNotBeNull();
        buildDefinitions.First().Id.ShouldNotBeNull();
        buildDefinitions.First().Project.Id.ShouldNotBeNull();
        buildDefinitions.First().Links.ShouldNotBeNull();
    }

    [Fact]
    public async Task QueryBuildDefinitionsReturnsBuildDefinitionsWithExtendedProperties()
    {
        var projectId = (await _client.GetAsync(Project.Properties(_config.ProjectName))).Id;

        var buildDefinitions = await _client.GetAsync(Builds.BuildDefinitions(projectId, true).Request.AsJson());

        buildDefinitions.ShouldNotBeNull();
        buildDefinitions.SelectTokens("value[*].process").Count().ShouldBeGreaterThan(0);
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task CanQueryBuildDefinitionsByProcessType()
    {
        var projectId = (await _client.GetAsync(Project.Properties(_config.ProjectName))).Id;

        var buildDefinitions = await _client.GetAsync(Builds.BuildDefinitions(projectId, 2));
        buildDefinitions.ToList();

        buildDefinitions.ShouldNotBeNull();
        buildDefinitions.First().Id.ShouldNotBeNull();
        buildDefinitions.First().Project.Id.ShouldNotBeNull();
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task GetProjectRetentionSetting()
    {
        var retentionSettings = await _client.GetAsync(Builds.Retention(_config.ProjectId),
            _config.Organization);

        retentionSettings.ShouldNotBeNull();
        retentionSettings.PurgeRuns.ShouldNotBeNull();
    }

    [Fact(Skip = "For manual testing only")]
    [Trait("category", "integration")]
    public async Task SetProjectRetentionSetting()
    {
        const int DaysToKeep = 450;
        var body = new SetRetention() { RunRetention = new RunRetention() { Value = DaysToKeep } };
        var retentionSettings = await _client.PatchAsync(Builds.SetRetention(_config.ProjectId),
            body, _config.Organization);

        retentionSettings.ShouldNotBeNull();
        retentionSettings.PurgeRuns.ShouldNotBeNull();
        retentionSettings.PurgeRuns.Value.ShouldBe(DaysToKeep);
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task QueryTimeline()
    {
        var timeline = await _client.GetAsync(Builds.Timeline(_config.ProjectName, _config.BuildId));
        timeline.ShouldNotBeNull();
        timeline.Records.ShouldNotBeNull();
        timeline.Records.Count().ShouldBeGreaterThan(0);
        timeline.Records.All(r => Guid.Empty != r.Id).ShouldBeTrue();
        timeline.Records.All(r => !string.IsNullOrEmpty(r.Name)).ShouldBeTrue();
        timeline.Records.All(r => !string.IsNullOrEmpty(r.Type)).ShouldBeTrue();
    }

    [Fact]
    public async Task NotFoundIsNull()
    {
        var result = await _client.GetAsync(Builds.Build(_config.ProjectName, "0"));
        result.ShouldBeNull();
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task AddAndRemoveTags()
    {
        const string tag = "Test tag";

        var tagsBefore = await _client.GetAsync(Builds.Tags(_config.ProjectId, _config.BuildId));
        tagsBefore.Value.Contains(tag).ShouldBeFalse();

        var tagsAfterAdding = await _client.PutAsync(Builds.Tag(_config.ProjectId, _config.BuildId, tag), null);
        tagsAfterAdding.Value.Contains(tag).ShouldBeTrue();

        await _client.DeleteAsync(Builds.Tag(_config.ProjectId, _config.BuildId, tag));
        var tagsAfterRemoving = await _client.GetAsync(Builds.Tags(_config.ProjectId, _config.BuildId));
        tagsAfterRemoving.Value.Contains(tag).ShouldBeFalse();
    }

    [Fact]
    public async Task GetAsStringAsyncShouldGetLogsAsString()
    {
        // Arrange

        // Act
        var response = await _client.GetAsStringAsync(Builds.GetLogs1(_config.ProjectId, int.Parse(_config.BuildId)));

        // Assert
        Assert.Contains("stage", response, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("jobs", response, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetLogs_ShouldGetLogs()
    {
        // Arrange

        // Act
        var response = await _client.GetAsStringAsync(Builds.GetLogs(_config.ProjectId, "336474", 9));

        // Assert
        response.ShouldNotBeNull();
        response.ShouldContain("Pre-job: check pipeline registration and compliancy");
        response.ShouldContain("ERROR: This pipeline is not registered");
    }
}
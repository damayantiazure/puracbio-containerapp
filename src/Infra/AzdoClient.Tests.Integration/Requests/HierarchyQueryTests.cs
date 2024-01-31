using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System;
using System.Linq;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

[Trait("category", "integration")]
public class HierarchyQueryTests : IClassFixture<HierarchyQueryTests.PipelineFixture>
{
    private readonly PipelineFixture _fixture;
    private readonly IAzdoRestClient _azdoClient;

    public HierarchyQueryTests(PipelineFixture fixture)
    {
        _fixture = fixture;
        _azdoClient = new AzdoRestClient(_fixture.Token);
    }

    #region [README | Query Approvals]
    // To query approvals, we need the stage ID which is only available via a REST call
    // $uri = "$(System.CollectionUri)/$(System.TeamProject)/_apis/build/builds/$(Build.BuildID)/timeline?api-version=6.0"
    // $response = Get-WebClient.DownloadString($uri) | ConvertFrom-Json
    // $stage = $response.records | where-object -property name -eq "$(System.StageName)"
    #endregion

    [Fact]
    public async Task QueryApprovals()
    {
        var projectInfo = HierarchyQuery.ProjectInfo(_fixture.ProjectId);
        var approvalsRequest = HierarchyQuery.Approvals(_fixture.ProjectName, "336474", "de00afe2-3b4b-5d17-8d57-d20dedd3fa47");
        var approvals = await _fixture.Client.PostAsync(projectInfo, approvalsRequest);

        approvals.ShouldNotBeNull();

        var approvers = approvals.SelectTokens("dataProviders.['ms.vss-build-web.checks-panel-data-provider'][0].approvals[*].steps[*].actualApprover.id")
            .Select(x => x.ToString());

        approvers.Any().ShouldBeTrue();
    }

    [Fact]
    public async Task QueryPipelineInfo()
    {
        var yamlInfo = await _azdoClient.PostAsync(HierarchyQuery.ProjectInfo(_fixture.ProjectId),
            HierarchyQuery.PipelineVersion(_fixture.PipelineId, _fixture.Branch, _fixture.ProjectName), _fixture.Organization, true);

        yamlInfo.ShouldNotBeNull();

        var stages = yamlInfo
            .SelectTokens("dataProviders.['ms.vss-build-web.pipeline-run-parameters-data-provider'].stages[*].refName")
            .Select(x => new Response.Stage { Id = x.ToString(), Name = x.ToString() })
            .ToList();

        stages.Any().ShouldBeTrue();
    }

    public class PipelineFixture : IDisposable
    {
        public string Organization { get; }
        public string ProjectId { get; }
        public string ProjectName { get; }
        public string RunId { get; }
        public string StageId { get; }
        public string PipelineId { get; }
        public string Branch { get; }
        public string QueryPipelineInfoProjectName { get; }
        public string QueryPipelineInfoProjectId { get; }
        public string QueryPipelineInfoPipelineId { get; }
        public string Token { get; }
        public IAzdoRestClient Client { get; }

        public PipelineFixture()
        {
            var config = new TestConfig();

            Client = new AzdoRestClient(config.Organization, config.Token);
            ProjectId = config.ProjectId;
            ProjectName = config.ProjectName;
            RunId = config.BuildId;
            StageId = config.StageId;
            PipelineId = config.PipelineId;
            Branch = config.Branch;
            QueryPipelineInfoProjectName = config.QueryPipelineInfoProjectName;
            QueryPipelineInfoProjectId = config.QueryPipelineInfoProjectId;
            QueryPipelineInfoPipelineId = config.QueryPipelineInfoPipelineId;
            Organization = config.Organization;
            Token = config.Token;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
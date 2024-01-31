using Microsoft.Azure.Pipelines.WebApi;
using Microsoft.TeamFoundation.Build.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Tests;
using Rabobank.Compliancy.Tests.Helpers;
using System.Net.Http.Headers;
using Microsoft.VisualStudio.Services.WebApi;
using Run = Microsoft.Azure.Pipelines.WebApi.Run;
using AutoFixture.AutoMoq;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class PipelineRepositoryTests : UnitTestBase
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    private readonly Mock<IDevHttpClientCallHandler> _httpClientCallHandlerMock = new();

    private readonly PipelineRepository _sut;

    public PipelineRepositoryTests() => _sut = new PipelineRepository(_httpClientCallHandlerMock.Object);

    [Fact]
    public async Task GetPipelineYamlFromPreviewRunAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var securedObject = _fixture.Create<ISecuredObject>();
        var previewRun = new PreviewRun(securedObject) { FinalYaml = "yaml" };
        _httpClientCallHandlerMock.Setup(x => x.HandlePostCallAsync<PreviewRun, object>(It.IsAny<Uri>(), It.IsAny<object>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(previewRun);

        // Act
        var actual = await _sut.GetPipelineYamlFromPreviewRunAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<int>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(previewRun.FinalYaml);
    }

    [Fact]
    public async Task GetPipelineRunAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var run = (Run)Activator.CreateInstance(typeof(Run), true)!;
        run.CreatedDate = DateTime.Now;
        run.FinalYaml = "Test";
        run.FinishedDate = DateTime.Now;
        run.Id = 1;
        run.Name = "Test";
        run.Result = RunResult.Succeeded;
        run.State = RunState.Completed;

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<Run>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(run);

        // Act
        var actual = await _sut.GetPipelineRunAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<int>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(run);
    }

    [Fact]
    public async Task GetBuildApproverByTimelineAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var uniqueNames = new[] { "UN1", "UN2", "UN3", "UN4", "UN5", "UN6" };
        Approval[] approvals = CreateApprovals(uniqueNames);
        var approvalsCollectino = new ResponseCollection<Approval> { Count = approvals.Count(), Value = approvals };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<Approval>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approvalsCollectino).Verifiable();

        var timeLineArgument = (Timeline)Activator.CreateInstance(typeof(Timeline), true)!;
        timeLineArgument.SetPrivateFieldValue("m_records", new List<TimelineRecord> { new TimelineRecord { RecordType = "Checkpoint.Approval", Identifier = Guid.NewGuid().ToString() } });

        // Act
        var actual = await _sut.GetBuildApproverByTimelineAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), timeLineArgument, It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotContain(uniqueNames[1]).And.NotContain(uniqueNames[2]).And.NotContain(uniqueNames[3]);
        actual.Should().Contain(uniqueNames[0]).And.Contain(uniqueNames[4]).And.Contain(uniqueNames[5]);
    }

    [Fact]
    public async Task GetBuildApproverByTimelineAsync_WithNullTimelineParameter_ShouldBeNull()
    {
        // Act
        var actual = await _sut.GetBuildApproverByTimelineAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), null, It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeNull();
    }

    [Fact]
    public async Task GetBuildApproverByTimelineAsync_WithoutTimelineRecords_ShouldBeNull()
    {
        // Arrange
        var timeLineArgument = (Timeline)Activator.CreateInstance(typeof(Timeline), true)!; // Initiate with empty collection
        timeLineArgument.SetPrivateFieldValue<List<TimelineRecord>>("m_records", null); // So set it to null

        // Act
        var actual = await _sut.GetBuildApproverByTimelineAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), timeLineArgument, It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeNull();
    }

    [Fact]
    public async Task GetBuildApproverByTimelineAsync_WithEmptyTimelineRecords_ShouldBeNull()
    {
        // Arrange
        var timeLineArgument = (Timeline)Activator.CreateInstance(typeof(Timeline), true)!; // Initiate with empty collection

        // Act
        var actual = await _sut.GetBuildApproverByTimelineAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), timeLineArgument, It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeNull();
    }

    [Fact]
    public async Task GetBuildApproverByTimelineAsync_WithNullApprovals_ShouldBeNulle()
    {
        // Arrange
        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<Approval>?>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResponseCollection<Approval>?)null).Verifiable();

        var timeLineArgument = (Timeline)Activator.CreateInstance(typeof(Timeline), true)!;
        timeLineArgument.SetPrivateFieldValue("m_records", new List<TimelineRecord> { new TimelineRecord { RecordType = "Checkpoint.Approval", Identifier = Guid.NewGuid().ToString() } });

        // Act
        var actual = await _sut.GetBuildApproverByTimelineAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), timeLineArgument, It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeNull();
    }

    private static Approval[] CreateApprovals(string[] uniqueNames)
    {
        return new[]
        {
            new Approval
            {
                Id = Guid.NewGuid(),
                Status = "approved",
                Steps = new[]
                {
                    new ApprovalStep { AssignedApprover = new Approver { UniqueName = uniqueNames[0] }, Status = "approved" },
                    new ApprovalStep { AssignedApprover = new Approver { UniqueName = uniqueNames[1] }, Status = "NotApproved" }
                }
            },
            new Approval
            {
                Id = Guid.NewGuid(),
                Status = "notApproved",
                Steps = new[]
                {
                    new ApprovalStep { AssignedApprover = new Approver { UniqueName = uniqueNames[2] }, Status = "approved" },
                    new ApprovalStep { AssignedApprover = new Approver { UniqueName = uniqueNames[3] }, Status = "NotApproved" }
                }
            },
            new Approval
            {
                Id = Guid.NewGuid(),
                Status = "approved",
                Steps = new[]
                {
                    new ApprovalStep { AssignedApprover = new Approver { UniqueName = uniqueNames[4] }, Status = "approved" },
                    new ApprovalStep { AssignedApprover = new Approver { UniqueName = uniqueNames[5] }, Status = "approved" }
                }
            }
        };
    }
}
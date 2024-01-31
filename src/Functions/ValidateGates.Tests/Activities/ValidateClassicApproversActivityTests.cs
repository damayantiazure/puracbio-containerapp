#nullable enable

using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Functions.ValidateGates.Activities;
using Rabobank.Compliancy.Functions.ValidateGates.Model;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ValidateGates.Tests.Activities;

public class ValidateClassicApproversActivityTests
{
    private readonly Mock<IPullRequestApproverService> _pullRequestApproverService = new();
    private readonly Mock<IClassicReleaseApproverService> _classicReleaseApprovalService = new();
    private const int _id = 123;
    private const string _projectId = "tas";
    private const string _organization = "rabobank-test";
    private const string _runId = "3456";
    private const string _versionId = "890";

    [Fact]
    public async Task ValidateClassicApproversActivity_RunAsync_NoApprovers()
    {
        //assert
        var release = new Release
        {
            Id = _id,
            CreatedBy = new IdentityRef { Id = Guid.NewGuid() },
            Artifacts = new List<ArtifactReference> 
            {
                new()
                {
                    Type = "Build",
                    DefinitionReference = new DefinitionReference
                    {
                        Version = new Infra.AzdoClient.Response.Version { Id = _versionId }
                    }
                }
            } 
        };

        var input = (_projectId, release, _organization);

        _classicReleaseApprovalService.Setup(m => m.HasApprovalAsync(_projectId, release.Id.ToString(), release.CreatedBy.Id.ToString(), _organization)).ReturnsAsync(false);

        _pullRequestApproverService.Setup(m => m.HasApprovalAsync(_projectId, _runId, _organization));

        var function = new ValidateClassicApproversActivity(_pullRequestApproverService.Object, _classicReleaseApprovalService.Object);
        var result = await function.RunAsync(input);

        Assert.Equal(ApprovalType.NoApproval, result.DeterminedApprovalType);

    }

    [Fact]
    public async Task ValidateClassicApproversActivity_RunAsync_HasPipelineApproval()
    {
        //assert
        var release = new Release
        {
            Id = _id,
            CreatedBy = new IdentityRef { Id = Guid.NewGuid() },
            Artifacts = new List<ArtifactReference>
            {
                new()
                {
                    Type = "Build",
                    DefinitionReference = new DefinitionReference
                    {
                        Version = new Infra.AzdoClient.Response.Version { Id = _versionId }
                    }
                }
            }
        };

        var input = (_projectId, release, _organization);

        _classicReleaseApprovalService.Setup(m => m.HasApprovalAsync(_projectId, release.Id.ToString(), release.CreatedBy.Id.ToString(), _organization)).ReturnsAsync(true);

        _pullRequestApproverService.Setup(m => m.HasApprovalAsync(_projectId, _runId, _organization));

        var function = new ValidateClassicApproversActivity(_pullRequestApproverService.Object, _classicReleaseApprovalService.Object);
        var result = await function.RunAsync(input);

        Assert.Equal(ApprovalType.PipelineApproval, result.DeterminedApprovalType);

    }

    [Fact]
    public async Task ValidateClassicApproversActivity_RunAsync_HasPullRequestApproval()
    {
        //assert
        var release = new Release
        {
            Id = _id,
            CreatedBy = new IdentityRef { Id = Guid.NewGuid() },
            Artifacts = new List<ArtifactReference>
            {
                new()
                {
                    Type = "Build",
                    DefinitionReference = new DefinitionReference
                    {
                        Version = new Infra.AzdoClient.Response.Version { Id = _versionId }
                    }
                }
            }
        };

        var input = (_projectId, release, _organization);

        _classicReleaseApprovalService.Setup(m => m.HasApprovalAsync(_projectId, release.Id.ToString(), release.CreatedBy.Id.ToString(), _organization)).ReturnsAsync(false);

        _pullRequestApproverService.Setup(m => m.HasApprovalAsync(_projectId, _versionId, _organization)).ReturnsAsync(true);

        var function = new ValidateClassicApproversActivity(_pullRequestApproverService.Object, _classicReleaseApprovalService.Object);
        var result = await function.RunAsync(input);

        Assert.Equal(ApprovalType.PullRequestApproval, result.DeterminedApprovalType);
    }
}
#nullable enable

using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Functions.ValidateGates.Activities;
using Rabobank.Compliancy.Functions.ValidateGates.Model;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ValidateGates.Tests.Activities;

public class ValidateYamlApproversActivityTests
{
    private readonly Mock<IAzdoRestClient> _azdoRestClient;
    private readonly Mock<IYamlReleaseApproverService> _yamlReleaseApproverService;
    private readonly Mock<IPullRequestApproverService> _pullRequestApproverService;


    public ValidateYamlApproversActivityTests()
    {
        _azdoRestClient = new Mock<IAzdoRestClient>();
        _yamlReleaseApproverService = new Mock<IYamlReleaseApproverService>();
        _pullRequestApproverService = new Mock<IPullRequestApproverService>();
    }

    [Fact]

    public async Task ValidateYamlApproversActivity_RunAsync_NoApproval()
    {
        var projecId = "tas";
        var runId = "123";
        var organization = "raboweb-test";
        var input = (projecId, runId, organization);

        var project = new Project { Id = "123"};
        var build = new Build { Id = 123, RequestedFor = new RequestedFor { UniqueName = "God" } };

        _pullRequestApproverService.Setup(m => m.HasApprovalAsync(projecId, runId, organization)).ReturnsAsync(false);
        _azdoRestClient
            .Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Project>>(), It.IsAny<string>()))
            .ReturnsAsync(project);
        _azdoRestClient
            .Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()))
            .ReturnsAsync(build);
        _yamlReleaseApproverService
            .Setup(x => x.HasApprovalAsync(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var function = new ValidateYamlApproversActivity(_azdoRestClient.Object, _yamlReleaseApproverService.Object, _pullRequestApproverService.Object);
        var result = await function.RunAsync(input);

        Assert.Equal(ApprovalType.NoApproval, result.DeterminedApprovalType);
    }

    [Fact]

    public async Task ValidateYamlApproversActivity_RunAsync_PipelineApproval()
    {
        var projecId = "tas";
        var runId = "123";
        var organization = "raboweb-test";
        var input = (projecId, runId, organization);

        var project = new Project { Id = "123" };
        var build = new Build { Id = 123, RequestedFor = new RequestedFor { UniqueName = "God" } };

        _pullRequestApproverService.Setup(m => m.HasApprovalAsync(projecId, runId, organization)).ReturnsAsync(false);
        _azdoRestClient
            .Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Project>>(), It.IsAny<string>()))
            .ReturnsAsync(project);
        _azdoRestClient
            .Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()))
            .ReturnsAsync(build);
        _yamlReleaseApproverService
            .Setup(x => x.HasApprovalAsync(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var function = new ValidateYamlApproversActivity(_azdoRestClient.Object, _yamlReleaseApproverService.Object, _pullRequestApproverService.Object);
        var result = await function.RunAsync(input);

        Assert.Equal(ApprovalType.PipelineApproval, result.DeterminedApprovalType);
    }

    [Fact]

    public async Task ValidateYamlApproversActivity_RunAsync_PullRequestApproval()
    {
        var projecId = "tas";
        var runId = "123";
        var organization = "raboweb-test";
        var input = (projecId, runId, organization);

        var project = new Project { Id = "123" };
        var build = new Build { Id = 123, RequestedFor = new RequestedFor { UniqueName = "God" } };

        _pullRequestApproverService.Setup(m => m.HasApprovalAsync(projecId, runId, organization)).ReturnsAsync(true);
        _azdoRestClient
            .Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Project>>(), It.IsAny<string>()))
            .ReturnsAsync(project);
        _azdoRestClient
            .Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()))
            .ReturnsAsync(build);
        _yamlReleaseApproverService
            .Setup(x => x.HasApprovalAsync(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var function = new ValidateYamlApproversActivity(_azdoRestClient.Object, _yamlReleaseApproverService.Object, _pullRequestApproverService.Object);
        var result = await function.RunAsync(input);

        Assert.Equal(ApprovalType.PullRequestApproval, result.DeterminedApprovalType);
    }
}
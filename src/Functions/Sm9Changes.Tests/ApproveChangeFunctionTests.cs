#nullable enable

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.Sm9Changes.Model;
using Rabobank.Compliancy.Functions.Sm9Changes.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.Sm9Client.Change;
using Rabobank.Compliancy.Infra.Sm9Client.Change.Model;
using System;
using System.Net.Http;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Tests;

public class ApproveChangeFunctionTests
{
    private const string _changeId = "C012345678";

    private const string _changeUrl = "https://itsm.rabobank.nl/SM/index.do?ctx=docEngine&file=cm3r&" +
                                     "query=number%3D%22C001102902%22&action=&title=Change%20Request%20Details&queryHash=eb7f36";

    private readonly Mock<IAzdoRestClient> _azdoClientMock = new();
    private readonly Mock<IChangeClient> _changeClientMock = new();
    private readonly Mock<IClassicReleaseApproverService> _classicReleaseApproversServiceMock = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IPullRequestApproverService> _pullRequestApproversServiceMock = new();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();
    private readonly Mock<IYamlReleaseApproverService> _yamlReleaseApproversServiceMock = new();
    private ISm9ChangesService _sm9ChangesService;

    public ApproveChangeFunctionTests() =>
        _sm9ChangesService = new Sm9ChangesService(_changeClientMock.Object);

    [Fact]
    public async Task OrganizationNotProvided_ReturnsBadRequest()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), null, _fixture.Create<string>(),
            SM9Constants.BuildPipelineType, _fixture.Create<string>());

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>()
            , It.IsAny<Exception>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("'organization' is not provided in the request url");
    }

    [Fact]
    public async Task ProjectIdNotProvided_ReturnsBadRequest()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _fixture.Create<string>(), null,
            SM9Constants.BuildPipelineType, _fixture.Create<string>());

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(
            LogDestinations.Sm9ChangesErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(),
            It.IsAny<Exception>()), Times.Once);
        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("'projectId' is not provided in the request url");
    }

    [Fact]
    public async Task PipelineTypeNotProvided_ReturnsBadRequest()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _fixture.Create<string>(),
            _fixture.Create<string>(), null, _fixture.Create<string>());

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(
            LogDestinations.Sm9ChangesErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(),
            It.IsAny<Exception>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("'pipelineType' is not provided in the request url");
    }

    [Fact]
    public async Task PipelineTypeInvalid_ReturnsBadRequest()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _fixture.Create<string>(),
            _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(
            LogDestinations.Sm9ChangesErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(),
            It.IsAny<Exception>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("An invalid 'pipelineType' has been provided in the request url.");
    }

    [Fact]
    public async Task RunIdNotProvided_ReturnsBadRequest()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _fixture.Create<string>(),
            _fixture.Create<string>(), SM9Constants.BuildPipelineType, null);

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(
            LogDestinations.Sm9ChangesErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(),
            It.IsAny<Exception>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("'runId' is not provided in the request url");
    }

    [Fact]
    public async Task RequestContentNotProvided_ReturnsBadRequest()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(new HttpRequestMessage(), _fixture.Create<string>(),
            _fixture.Create<string>(), SM9Constants.BuildPipelineType, _fixture.Create<string>());

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(
                LogDestinations.Sm9ChangesErrorLog,
                It.IsAny<ExceptionBaseMetaInformation>(),
                It.IsAny<Exception>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("'Content' is not provided in the request message");
    }

    [Fact]
    public async Task ChangeIdInvalid_ReturnsBadRequest()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(false),
            _fixture.Create<string>(), _fixture.Create<string>(), SM9Constants.BuildPipelineType,
            _fixture.Create<string>());

        // Assert
        _azdoClientMock.Verify(a => a.GetAsync(
            It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain(
            "No valid Change Id has been provided via either pipeline tags or pipeline variables.");
    }

    [Fact]
    public async Task ChangeIdIncorrectPhase_ReturnsBadRequest()
    {
        // Arrange
        _changeClientMock
            .Setup(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()))
            .ReturnsAsync(_fixture.Create<GetChangeByKeyResponse>());

        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _fixture.Create<string>(),
            _fixture.Create<string>(), SM9Constants.BuildPipelineType, _fixture.Create<string>());

        // Assert
        _changeClientMock
            .Verify(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()), Times.Once);
        _yamlReleaseApproversServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        _pullRequestApproversServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain($"The following Changes do not have the correct Change Phase: {_changeId}");
    }

    [Fact]
    public async Task NoApprovers_ReturnsBadRequest()
    {
        // Arrange
        _fixture.Customize<ChangeInformation>(composer => composer
            .With(g => g.Phase, SM9Constants.DeploymentPhase));

        _changeClientMock
            .Setup(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()))
            .ReturnsAsync(_fixture.Create<GetChangeByKeyResponse>());

        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _fixture.Create<string>(),
            _fixture.Create<string>(), SM9Constants.BuildPipelineType, _fixture.Create<string>());

        // Assert
        _changeClientMock
            .Verify(c => c.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()), Times.Once);
        _azdoClientMock
            .Verify(a => a.GetAsync(It.IsAny<IAzdoRequest<Project>>(), It.IsAny<string>()), Times.Once);
        _yamlReleaseApproversServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _pullRequestApproversServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("Neither a pull request approval nor a pipeline approval has been provided.");
    }

    [Fact]
    public async Task NoPipelineUrl_ReturnsBadRequest()
    {
        // Arrange
        _fixture.Customize<ChangeInformation>(composer => composer
            .With(i => i.Phase, SM9Constants.DeploymentPhase));

        _changeClientMock
            .Setup(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()))
            .ReturnsAsync(_fixture.Create<GetChangeByKeyResponse>());

        _yamlReleaseApproversServiceMock
            .Setup(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.CreateMany<string>());

        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _fixture.Create<string>(),
            _fixture.Create<string>(), SM9Constants.BuildPipelineType, _fixture.Create<string>());

        // Assert
        _changeClientMock
            .Verify(c => c.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()), Times.Once);
        _azdoClientMock
            .Verify(a => a.GetAsync(It.IsAny<IAzdoRequest<Project>>(), It.IsAny<string>()), Times.Once);
        _yamlReleaseApproversServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _pullRequestApproversServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _changeClientMock
            .Verify(c => c.UpdateChangeAsync(It.IsAny<UpdateChangeRequestBody>()), Times.Once);
        _azdoClientMock
            .Verify(a => a.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()), Times.Once);
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(
            LogDestinations.Sm9ChangesErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(),
            It.IsAny<Exception>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("Pipeline url could not be retrieved from Azure DevOps.");
    }

    [Fact]
    public async Task NoChangeUrlHash_ReturnsBadRequest()
    {
        // Arrange
        _fixture.Customize<ChangeInformation>(g => g
            .With(i => i.Phase, SM9Constants.DeploymentPhase)
            .With(i => i.Url, _fixture.Create<Uri>().AbsoluteUri));

        _changeClientMock
            .Setup(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()))
            .ReturnsAsync(_fixture.Create<GetChangeByKeyResponse>());

        _azdoClientMock
            .Setup(a => a.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<Build>());

        _yamlReleaseApproversServiceMock
            .Setup(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.CreateMany<string>());

        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _fixture.Create<string>(),
            _fixture.Create<string>(), SM9Constants.BuildPipelineType, _fixture.Create<string>());

        // Assert
        _azdoClientMock
            .Verify(a => a.GetAsync(It.IsAny<IAzdoRequest<Project>>(), It.IsAny<string>()), Times.Once);
        _yamlReleaseApproversServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _pullRequestApproversServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _changeClientMock
            .Verify(c => c.UpdateChangeAsync(It.IsAny<UpdateChangeRequestBody>()), Times.Exactly(2));
        _azdoClientMock
            .Verify(a => a.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()), Times.Once);
        _changeClientMock
            .Verify(c => c.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()), Times.Exactly(2));

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("No Change URL with hash received from SM9 GetChangeByKey API call.");
    }

    [Fact]
    public async Task Build_ChangeIdViaInput_IsApprovedAndReturnsOkObjectResult()
    {
        // Arrange
        _fixture.Customize<ChangeInformation>(composer => composer
            .With(i => i.Url, _changeUrl)
            .With(i => i.Phase, SM9Constants.DeploymentPhase));

        _changeClientMock
            .Setup(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()))
            .ReturnsAsync(_fixture.Create<GetChangeByKeyResponse>());

        _azdoClientMock
            .Setup(a => a.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<Build>());

        _yamlReleaseApproversServiceMock
            .Setup(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.CreateMany<string>());

        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _fixture.Create<string>(),
            _fixture.Create<string>(), SM9Constants.BuildPipelineType, _fixture.Create<string>());

        // Assert
        _azdoClientMock
            .Verify(a => a.GetAsync(It.IsAny<IAzdoRequest<Project>>(), It.IsAny<string>()), Times.Once);
        _yamlReleaseApproversServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _pullRequestApproversServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _changeClientMock
            .Verify(c => c.UpdateChangeAsync(It.IsAny<UpdateChangeRequestBody>()), Times.Exactly(2));
        _azdoClientMock
            .Verify(a => a.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()), Times.Once);
        _changeClientMock
            .Verify(c => c.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()), Times.Exactly(2));
        _azdoClientMock
            .Verify(a => a.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()), Times.Once);
        _azdoClientMock
            .Verify(a => a.PutAsync(It.IsAny<IAzdoRequest<Tags?>>(), null, It.IsAny<string>()), Times.Once);

        actual.ShouldBeOfType<OkObjectResult>();
        var resultValue = ((OkObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain(
            $"Azure DevOps approvals and the pipeline url have been logged for changes: {_changeId}");
    }

    [Fact]
    public async Task Build_ChangeIdViaTags_IsApprovedAndReturnsOkObjectResult()
    {
        // Arrange
        _fixture.Customize<ChangeInformation>(composer => composer
            .With(i => i.Url, _changeUrl)
            .With(i => i.Phase, SM9Constants.DeploymentPhase));

        _changeClientMock
            .Setup(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()))
            .ReturnsAsync(_fixture.Create<GetChangeByKeyResponse>());

        _azdoClientMock
            .Setup(m => m.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()))
            .ReturnsAsync(new Tags { Value = new[] { _changeId } });
        _azdoClientMock
            .Setup(a => a.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<Build>());

        _yamlReleaseApproversServiceMock
            .Setup(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(_fixture.CreateMany<string>());

        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(false),
            _fixture.Create<string>(), _fixture.Create<string>(), SM9Constants.BuildPipelineType,
            _fixture.Create<string>());

        // Assert
        _azdoClientMock
            .Verify(a => a.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()), Times.Exactly(2));
        _azdoClientMock
            .Verify(a => a.GetAsync(It.IsAny<IAzdoRequest<Project>>(), It.IsAny<string>()), Times.Once);
        _yamlReleaseApproversServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _pullRequestApproversServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _changeClientMock
            .Verify(c => c.UpdateChangeAsync(It.IsAny<UpdateChangeRequestBody>()), Times.Exactly(2));
        _azdoClientMock
            .Verify(a => a.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()), Times.Once);
        _changeClientMock
            .Verify(c => c.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()), Times.Exactly(2));
        _azdoClientMock
            .Verify(a => a.DeleteAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()), Times.Once);
        _azdoClientMock
            .Verify(a => a.PutAsync(It.IsAny<IAzdoRequest<Tags?>>(), null, It.IsAny<string>()), Times.Once);

        actual.ShouldBeOfType<OkObjectResult>();
        var resultValue = ((OkObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain(
            $"Azure DevOps approvals and the pipeline url have been logged for changes: {_changeId}");
    }

    [Fact]
    public async Task Release_ChangeIdViaTags_IsApprovedAndReturnsOkObjectResult()
    {
        // Arrange
        _fixture.Customize<ChangeInformation>(composer => composer
            .With(i => i.Url, _changeUrl)
            .With(i => i.Phase, SM9Constants.DeploymentPhase));
        _fixture.Customize<ArtifactReference>(composer => composer
            .With(a => a.Type, "Build"));

        _changeClientMock
            .Setup(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()))
            .ReturnsAsync(_fixture.Create<GetChangeByKeyResponse>());
        _azdoClientMock
            .Setup(m => m.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()))
            .ReturnsAsync(new Tags { Value = new[] { _changeId } });
        _azdoClientMock
            .Setup(a => a.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<Release>());

        _classicReleaseApproversServiceMock
            .Setup(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(_fixture.CreateMany<string>());

        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(false),
            _fixture.Create<string>(), _fixture.Create<string>(), SM9Constants.ReleasePipelineType,
            _fixture.Create<string>());

        // Assert
        _azdoClientMock
            .Verify(a => a.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()), Times.Exactly(2));
        _classicReleaseApproversServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _azdoClientMock
            .Verify(a => a.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()), Times.Exactly(2));
        _pullRequestApproversServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Exactly(3));
        _changeClientMock
            .Verify(c => c.UpdateChangeAsync(It.IsAny<UpdateChangeRequestBody>()), Times.Exactly(2));
        _changeClientMock
            .Verify(c => c.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()), Times.Exactly(2));
        _azdoClientMock
            .Verify(a => a.DeleteAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()), Times.Once);
        _azdoClientMock
            .Verify(a => a.PatchAsync(It.IsAny<IAzdoRequest<Tags?>>(), null, It.IsAny<string>()), Times.Once);

        actual.ShouldBeOfType<OkObjectResult>();
        var resultValue = ((OkObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain(
            $"Azure DevOps approvals and the pipeline url have been logged for changes: {_changeId}");
    }

    [Fact]
    public async Task RunAsync_WithUnexpectedException_ShouldThrowException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var runId = _fixture.Create<string>();

        var sm9ChangesServiceMock = new Mock<ISm9ChangesService>();
        sm9ChangesServiceMock.Setup(m => m.ValidateFunctionInput(
                It.IsAny<HttpRequestMessage>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()))
            .Throws<InvalidOperationException>();

        _sm9ChangesService = sm9ChangesServiceMock.Object;

        var sut = CreateSut();

        // Act
        var actual = () => sut.RunAsync(CreateHttpRequestWithContent(), organization, projectId,
            SM9Constants.BuildPipelineType, runId);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock
            .Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
                It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<Exception>()),
                Times.Once);
    }

    private ApproveChangeFunction CreateSut() =>
        new(_azdoClientMock.Object, _changeClientMock.Object,
            _loggingServiceMock.Object, _sm9ChangesService, _yamlReleaseApproversServiceMock.Object,
            _classicReleaseApproversServiceMock.Object,
            _pullRequestApproversServiceMock.Object);

    private HttpRequestMessage CreateHttpRequestWithContent(bool validChangeId = true) =>
        new()
        {
            Content = new StringContent(JsonConvert.SerializeObject(
                new UpdateChangeRequestBody(validChangeId
                    ? _changeId
                    : _fixture.Create<string>())))
        };
}
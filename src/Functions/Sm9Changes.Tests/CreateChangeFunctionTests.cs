#nullable enable

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.Sm9Changes.Application;
using Rabobank.Compliancy.Functions.Sm9Changes.Model;
using Rabobank.Compliancy.Functions.Sm9Changes.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.Sm9Client.Change;
using Rabobank.Compliancy.Infra.Sm9Client.Change.Model;
using System;
using System.Linq;
using System.Net.Http;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Tests;

public class CreateChangeFunctionTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IAzdoRestClient> _azdoRestClientMock = new();
    private readonly Mock<IChangeClient> _changeClientMock = new();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();
    private readonly Mock<IYamlReleaseApproverService> _yamlReleaseApproverServiceMock = new();
    private readonly Mock<IClassicReleaseApproverService> _classicReleaseApproverServiceMock = new();
    private readonly Mock<IPullRequestApproverService> _pullRequestApproverServiceMock = new();
    private ISm9ChangesService _sm9ChangesService;
    private const string _changeId = "C012345678";
    private const string _initiator = "name@rabobank.nl";

    private const string _changeUrl = "https://itsm.rabobank.nl/SM/index.do?ctx=docEngine&file=cm3r&" +
                                     "query=number%3D%22C001102902%22&action=&title=Change%20Request%20Details&queryHash=eb7f36";

    private readonly string _pipelineType;
    private readonly string _organization;
    private readonly string _projectId;
    private readonly string _runId;

    public CreateChangeFunctionTests()
    {
        _sm9ChangesService = new Sm9ChangesService(_changeClientMock.Object);
        _pipelineType = SM9Constants.BuildPipelineType;
        _organization = _fixture.Create<string>();
        _projectId = Guid.NewGuid().ToString();
        _runId = "1234";
    }

    [Fact]
    public async Task RunAsync_WithInvalidOrganization_ShouldLogExceptionOnce()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await sut.RunAsync(CreateHttpRequestWithContent(), null, _projectId, _pipelineType, _runId);

        // Assert
        _loggingServiceMock.Verify(x =>
                x.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
                    It.Is<ExceptionBaseMetaInformation>(information =>
                        information.Function == nameof(CreateChangeFunction)
                        && information.Organization == null && information.ProjectId == _projectId &&
                        information.RequestUrl == null)
                    , It.IsAny<Exception>())
            , Times.Once);
    }

    [Fact]
    public async Task OrganizationNotProvided_ReturnsBadRequest()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), null, _projectId, _pipelineType, _runId);

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(),
            It.IsAny<Exception>()), Times.Once);

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
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _organization, null, _pipelineType, _runId);

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
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
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _organization, _projectId, null, _runId);

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
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
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _organization, _projectId,
            _fixture.Create<string>(), _runId);

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
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
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _organization, _projectId, _pipelineType, null);

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
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
        var result = await sut.RunAsync(new HttpRequestMessage(), _organization, _projectId, _pipelineType, _runId);

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(),
            It.IsAny<Exception>()), Times.Once);

        result.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)result).Value!.ToString();
        resultValue!.ShouldContain("'Content' is not provided in the request message");
    }

    [Fact]
    public async Task Build_NoApprovers_ReturnsBadRequest()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(),
            _organization, _projectId, _pipelineType, _runId);

        // Assert
        _yamlReleaseApproverServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _pullRequestApproverServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("Neither a pull request approval nor a pipeline approval has been provided.");
    }

    [Fact]
    public async Task Release_NoApprovers_ReturnsBadRequest()
    {
        // Arrange
        _fixture.Customize<ArtifactReference>(a => a
            .With(artifactReference => artifactReference.Type, "Build"));
        _azdoRestClientMock
            .Setup(a => a.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<Release>());
        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _organization, _projectId,
            SM9Constants.ReleasePipelineType, _runId);

        // Assert
        _classicReleaseApproverServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _pullRequestApproverServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Exactly(3));

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("Neither a pull request approval nor a pipeline approval has been provided.");
    }

    [Fact]
    public async Task Build_NoInitiator_ReturnsBadRequest()
    {
        // Arrange
        _azdoRestClientMock
            .Setup(a => a.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<Build>());
        _azdoRestClientMock
            .Setup(a => a.GetAsync(It.IsAny<IAzdoRequest<BuildDefinition>>(),
                It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<BuildDefinition>());
        _yamlReleaseApproverServiceMock
            .Setup(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(_fixture.CreateMany<string>());
        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _organization, _projectId, _pipelineType,
            _runId);

        // Assert
        _yamlReleaseApproverServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _pullRequestApproverServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(),
            It.IsAny<Exception>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("No pipeline initiator could be found.");
    }

    [Fact]
    public async Task NoChangeIdReturned_ReturnsBadRequest()
    {
        // Arrange
        _fixture.Customize<RequestedFor>(composer => composer
            .With(a => a.UniqueName, _initiator));
        _azdoRestClientMock
            .Setup(a => a.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<Build>());
        _yamlReleaseApproverServiceMock
            .Setup(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(_fixture.CreateMany<string>());
        _changeClientMock
            .Setup(x => x.CreateChangeAsync(It.IsAny<CreateChangeRequestBody>()))
            .ReturnsAsync(CreateChangeResponse(withId: false));
        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _organization, _projectId, _pipelineType,
            _runId);

        // Assert
        _changeClientMock
            .Verify(c => c.CreateChangeAsync(It.IsAny<CreateChangeRequestBody>()), Times.Once);
        _yamlReleaseApproverServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _pullRequestApproverServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("No Change ID received from SM9 Create Change API call.");
    }

    [Fact]
    public async Task NoChangeUrlReturned_ReturnsBadRequest()
    {
        // Arrange
        _fixture.Customize<RequestedFor>(a => a
            .With(requestedFor => requestedFor.UniqueName, _initiator));
        _azdoRestClientMock
            .Setup(a => a.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<Build>());
        _yamlReleaseApproverServiceMock
            .Setup(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(_fixture.CreateMany<string>());
        _changeClientMock
            .Setup(x => x.CreateChangeAsync(It.IsAny<CreateChangeRequestBody>()))
            .ReturnsAsync(CreateChangeResponse(withId: true));
        _changeClientMock
            .Setup(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()))
            .ReturnsAsync(CreateChangeByKeyResponse(withUrl: false));
        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(),
            _organization, _projectId, _pipelineType, _runId);

        // Assert
        _changeClientMock
            .Verify(c => c.CreateChangeAsync(It.IsAny<CreateChangeRequestBody>()), Times.Once);
        _changeClientMock
            .Verify(c => c.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()), Times.Once);
        _yamlReleaseApproverServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _pullRequestApproverServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("No Change URL with hash received from SM9 GetChangeByKey API call.");
    }

    [Fact]
    public async Task Build_ShouldCreateAndApproveChangeAndReturnOkObjectResult()
    {
        // Arrange
        _fixture.Customize<RequestedFor>(a => a
            .With(requestedFor => requestedFor.UniqueName, _initiator));
        _azdoRestClientMock
            .Setup(a => a.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<Build>());
        _yamlReleaseApproverServiceMock
            .Setup(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(_fixture.CreateMany<string>());
        _changeClientMock
            .Setup(x => x.CreateChangeAsync(It.IsAny<CreateChangeRequestBody>()))
            .ReturnsAsync(CreateChangeResponse(withId: true));
        _changeClientMock
            .Setup(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()))
            .ReturnsAsync(CreateChangeByKeyResponse(withUrl: true));
        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _organization, _projectId, _pipelineType,
            _runId);

        // Assert
        _yamlReleaseApproverServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _pullRequestApproverServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _changeClientMock
            .Verify(c => c.CreateChangeAsync(It.IsAny<CreateChangeRequestBody>()), Times.Once);
        _changeClientMock
            .Verify(c => c.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()), Times.Once);
        _changeClientMock
            .Verify(c => c.UpdateChangeAsync(It.IsAny<UpdateChangeRequestBody>()), Times.Once);
        _azdoRestClientMock
            .Verify(a => a.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()), Times.Once);
        _azdoRestClientMock
            .Verify(a => a.PutAsync(It.IsAny<IAzdoRequest<Tags?>>(), null, It.IsAny<string>()), Times.Once);

        actual.ShouldBeOfType<OkObjectResult>();
        var resultValue = ((OkObjectResult)actual).Value!.ToString();
        resultValue.ShouldBe(_changeId);
    }

    [Fact]
    public async Task Release_ShouldCreateAndApproveChangeAndReturnOkObjectResult()
    {
        // Arrange
        _fixture.Customize<IdentityRef>(a => a
            .With(identityRef => identityRef.UniqueName, _initiator));
        _azdoRestClientMock
            .Setup(a => a.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<Release>());
        _classicReleaseApproverServiceMock
            .Setup(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(_fixture.CreateMany<string>());
        _changeClientMock
            .Setup(x => x.CreateChangeAsync(It.IsAny<CreateChangeRequestBody>()))
            .ReturnsAsync(CreateChangeResponse(withId: true));
        _changeClientMock
            .Setup(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()))
            .ReturnsAsync(CreateChangeByKeyResponse(withUrl: true));
        var sut = CreateSut();

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _organization, _projectId,
            SM9Constants.ReleasePipelineType, _runId);

        // Assert
        _classicReleaseApproverServiceMock
            .Verify(x => x.GetAllApproversAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _changeClientMock
            .Verify(c => c.CreateChangeAsync(It.IsAny<CreateChangeRequestBody>()), Times.Once);
        _changeClientMock
            .Verify(c => c.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()), Times.Once);
        _changeClientMock
            .Verify(c => c.UpdateChangeAsync(It.IsAny<UpdateChangeRequestBody>()), Times.Once);
        _azdoRestClientMock
            .Verify(a => a.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()), Times.Once);
        _azdoRestClientMock
            .Verify(a => a.PatchAsync(It.IsAny<IAzdoRequest<Tags?>>(), null, It.IsAny<string>()), Times.Once);

        actual.ShouldBeOfType<OkObjectResult>();
        var resultValue = ((OkObjectResult)actual).Value!.ToString();
        resultValue.ShouldBe(_changeId);
    }

    [Fact]
    public async Task RunAsync_WithUnexpectedException_ShouldThrowException()
    {
        // Arrange
        var sm9ChangesServiceMock = new Mock<ISm9ChangesService>();
        sm9ChangesServiceMock.Setup(m => m.ValidateFunctionInput(
            It.IsAny<HttpRequestMessage>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>())).Throws<InvalidOperationException>();

        _sm9ChangesService = sm9ChangesServiceMock.Object;
        var sut = CreateSut();

        // Act
        var actual = () => sut.RunAsync(CreateHttpRequestWithContent(), _organization, _projectId,
            SM9Constants.ReleasePipelineType, _runId);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(),
            It.IsAny<Exception>()), Times.Once);
    }

    public CreateChangeFunction CreateSut() =>
        new(_azdoRestClientMock.Object, _changeClientMock.Object, _loggingServiceMock.Object,
            _sm9ChangesService, _yamlReleaseApproverServiceMock.Object, _classicReleaseApproverServiceMock.Object
            , _pullRequestApproverServiceMock.Object);

    private HttpRequestMessage CreateHttpRequestWithContent() =>
        new()
        {
            Content = new StringContent(JsonConvert.SerializeObject(
                new CreateChangeDetails
                {
                    PriorityTemplate = _fixture.Create<string>(),
                    ImplementationPlan = _fixture.Create<string[]>(),
                    Assets = _fixture.Create<string[]>(),
                    Title = _fixture.Create<string>(),
                    Description = _fixture.Create<string>()
                }))
        };

    private CreateChangeResponse CreateChangeResponse(bool withId = true) =>
        new()
        {
            Messages = _fixture.CreateMany<string>().ToArray(),
            ChangeData = new ChangeData
            {
                ChangeId = withId
                    ? _changeId
                    : null
            },
            ReturnCode = _fixture.Create<string>()
        };

    private GetChangeByKeyResponse CreateChangeByKeyResponse(bool withUrl = true) =>
        new()
        {
            Messages = _fixture.CreateMany<string>().ToArray(),
            RetrieveChangeInfoByKey = new ChangeByKey
            {
                Information = new[]
                {
                    new ChangeInformation
                    {
                        Url = withUrl
                            ? _changeUrl
                            : null
                    }
                }
            },
            ReturnCode = _fixture.Create<string>()
        };
}
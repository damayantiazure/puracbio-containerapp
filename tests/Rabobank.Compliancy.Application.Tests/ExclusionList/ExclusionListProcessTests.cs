#nullable enable
using FluentAssertions;
using Rabobank.Compliancy.Application.ExclusionList;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Exclusions;
using Rabobank.Compliancy.Domain.Exceptions;

namespace Rabobank.Compliancy.Application.Tests.ExclusionList;

public class ExclusionListProcessTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IExclusionService> _exclusionServiceMock = new();
    private readonly ExclusionListProcess _sut;
    private const string _exclusionUpdatedMessage = "Exclusion request succesfully approved.";
    private const string _exclusionCreatedMessage = "Exclusion has been registered. Please make sure you have someone else register an exclusion as well in order to finalize the exclusion!";

    public ExclusionListProcessTests()
    {
        _sut = new ExclusionListProcess(_exclusionServiceMock.Object);
    }

    [Fact]
    public async Task CreateOrUpdateExclusionListAsync_WithExpiredExclusion_ShouldCreateNewExclusionRecord()
    {
        // Arrange
        var exclusionListRequest = _fixture.Create<ExclusionListRequest>();
        var userId = _fixture.Create<Guid>();
        var user = _fixture.Build<User>().With(x => x.Id, userId).Create();
        var exclusion = _fixture.Build<Exclusion>().Without(x => x.Approver)
            .Without(x => x.Requester).With(x => x.Timestamp, DateTime.Now.AddHours(-30)).Create();

        _exclusionServiceMock.Setup(x => x.CreateOrUpdateExclusionAsync(It.Is<Exclusion>(x => x.Requester == user.MailAddress
            && x.ExclusionReasonRequester == exclusionListRequest.Reason), default)).Verifiable();

        _exclusionServiceMock.Setup(x => x.GetExclusionAsync(exclusionListRequest.Organization, exclusionListRequest.ProjectId,
            exclusionListRequest.PipelineId, exclusionListRequest.PipelineType, default)).ReturnsAsync(exclusion).Verifiable();

        // Act
        var actual = await _sut.CreateOrUpdateExclusionListAsync(exclusionListRequest, user);

        // Assert
        actual.Should().Be(_exclusionCreatedMessage);
        _exclusionServiceMock.Verify();
    }

    [Fact]
    public async Task CreateOrUpdateExclusionListAsync_WhenExclusionHasRunId_ShouldCreateNewExclusionRecord()
    {
        // Arrange
        var exclusionListRequest = _fixture.Create<ExclusionListRequest>();
        var userId = _fixture.Create<Guid>();
        var user = _fixture.Build<User>().With(x => x.Id, userId).Create();
        var exclusion = _fixture.Build<Exclusion>().With(x => x.Timestamp, DateTime.Now).Create();

        _exclusionServiceMock.Setup(x => x.CreateOrUpdateExclusionAsync(It.Is<Exclusion>(x => x.Requester == user.MailAddress
        && x.ExclusionReasonRequester == exclusionListRequest.Reason), default)).Verifiable();

        _exclusionServiceMock.Setup(x => x.GetExclusionAsync(exclusionListRequest.Organization, exclusionListRequest.ProjectId,
            exclusionListRequest.PipelineId, exclusionListRequest.PipelineType, default)).ReturnsAsync(exclusion).Verifiable();

        // Act
        var actual = await _sut.CreateOrUpdateExclusionListAsync(exclusionListRequest, user);

        // Assert
        actual.Should().Be(_exclusionCreatedMessage);
        _exclusionServiceMock.Verify();
    }

    [Fact]
    public async Task CreateOrUpdateExclusionListAsync_WithValidRequest_ShouldCreateNewExclusionRecord()
    {
        // Arrange
        var exclusionListRequest = _fixture.Create<ExclusionListRequest>();
        var userId = _fixture.Create<Guid>();
        var user = _fixture.Build<User>().With(x => x.Id, userId).Create();

        _exclusionServiceMock.Setup(x => x.CreateOrUpdateExclusionAsync(It.Is<Exclusion>(x => x.Requester == user.MailAddress
            && x.ExclusionReasonRequester == exclusionListRequest.Reason), default)).Verifiable();

        _exclusionServiceMock.Setup(x => x.GetExclusionAsync(exclusionListRequest.Organization, exclusionListRequest.ProjectId,
            exclusionListRequest.PipelineId, exclusionListRequest.PipelineType, default)).ReturnsAsync((Exclusion?)null).Verifiable();

        // Act
        var actual = await _sut.CreateOrUpdateExclusionListAsync(exclusionListRequest, user);

        // Assert
        actual.Should().Be(_exclusionCreatedMessage);
        _exclusionServiceMock.Verify();
    }

    [Fact]
    public async Task CreateOrUpdateExclusionListAsync_WithValidTimestamp_ShouldUpdateExclusionRecord()
    {
        // Arrange
        var exclusionListRequest = _fixture.Create<ExclusionListRequest>();
        var userId = _fixture.Create<Guid>();
        var user = _fixture.Build<User>().With(x => x.Id, userId).Create();
        var exclusion = _fixture.Build<Exclusion>().Without(x => x.Approver).With(x => x.Timestamp, DateTime.Now).Create();

        _exclusionServiceMock.Setup(x => x.CreateOrUpdateExclusionAsync(It.Is<Exclusion>(x => x.Approver == user.MailAddress
            && x.ExclusionReasonApprover == exclusionListRequest.Reason), default)).Verifiable();

        _exclusionServiceMock.Setup(x => x.GetExclusionAsync(exclusionListRequest.Organization, exclusionListRequest.ProjectId,
            exclusionListRequest.PipelineId, exclusionListRequest.PipelineType, default)).ReturnsAsync(exclusion).Verifiable();

        // Act
        var actual = await _sut.CreateOrUpdateExclusionListAsync(exclusionListRequest, user);

        // Assert
        actual.Should().Be(_exclusionUpdatedMessage);
        _exclusionServiceMock.Verify();
    }

    [Fact]
    public async Task CreateOrUpdateExclusionListAsync_ApprovalByTheSameUser_ShouldThrowInvalidExclusionRequesterException()
    {
        // Arrange
        var exclusionListRequest = _fixture.Create<ExclusionListRequest>();

        var userId = _fixture.Create<Guid>();
        var user = _fixture.Build<User>().With(x => x.Id, userId).Create();
        var exclusion = _fixture.Build<Exclusion>().With(x => x.Timestamp, DateTime.Now)
            .Without(x => x.RunId).With(x => x.Requester, user.MailAddress).Create();

        _exclusionServiceMock.Setup(x => x.GetExclusionAsync(exclusionListRequest.Organization, exclusionListRequest.ProjectId,
            exclusionListRequest.PipelineId, exclusionListRequest.PipelineType, default)).ReturnsAsync(exclusion).Verifiable();

        // Act
        var actual = () => _sut.CreateOrUpdateExclusionListAsync(exclusionListRequest, user);

        // Assert
        await actual.Should().ThrowAsync<InvalidExclusionRequesterException>();
        _exclusionServiceMock.Verify();
    }

    [Fact]
    public async Task CreateOrUpdateExclusionListAsync_WhenApproverAlreadyExists_ShouldThrowExclusionApproverAlreadyExistsException()
    {
        // Arrange
        var exclusionListRequest = _fixture.Create<ExclusionListRequest>();

        var userId = _fixture.Create<Guid>();
        var user = _fixture.Build<User>().With(x => x.Id, userId).Create();
        var exclusion = _fixture.Build<Exclusion>().With(x => x.Timestamp, DateTime.Now)
         .Without(x => x.RunId).Create();

        _exclusionServiceMock.Setup(x => x.GetExclusionAsync(exclusionListRequest.Organization, exclusionListRequest.ProjectId,
            exclusionListRequest.PipelineId, exclusionListRequest.PipelineType, default)).ReturnsAsync(exclusion).Verifiable();

        // Act
        var actual = () => _sut.CreateOrUpdateExclusionListAsync(exclusionListRequest, user);

        // Assert
        await actual.Should().ThrowAsync<ExclusionApproverAlreadyExistsException>();
        _exclusionServiceMock.Verify();
    }
}
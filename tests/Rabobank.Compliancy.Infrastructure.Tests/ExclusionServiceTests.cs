using AutoMapper;
using Azure;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.Exclusions;
using Rabobank.Compliancy.Domain.Compliancy.Exclusions;
using Rabobank.Compliancy.Infrastructure.Mapping;
using TableStorage.Abstractions.Store;

namespace Rabobank.Compliancy.Infrastructure.Tests;

public class ExclusionServiceTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly ExclusionService _sut;
    private readonly IMapper _mapper = CreateMapper();

    private readonly Mock<ITableStore<ExclusionEntity>> _repositoryMock = new();

    public ExclusionServiceTests() => 
        _sut = new ExclusionService(() => _repositoryMock.Object, _mapper);

    private static IMapper CreateMapper() =>
        new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ExclusionMappingProfile>();
        }));

    [Fact]
    public async Task GetExclusionAsync_WithExclusionRecord_ShouldRetrieveAndReturnExclusion()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<int>();
        var pipelineType = _fixture.Create<string>();

        var exclusionEntity = _fixture.Create<ExclusionEntity>();
        _repositoryMock.Setup(x => x.GetRecordAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(exclusionEntity);

        // Act
        var actual = await _sut.GetExclusionAsync(organization, projectId, pipelineId, pipelineType);

        // Assert
        actual.Should().NotBeNull();
        actual!.ProjectId.Should().Be(exclusionEntity.ProjectId);
        actual.Organization.Should().Be(exclusionEntity.Organization);
        actual.PipelineId.Should().Be(exclusionEntity.PipelineId);
        actual.PipelineType.Should().Be(exclusionEntity.PipelineType);
        actual.RunId.Should().Be(exclusionEntity.RunId);
        actual.Requester.Should().Be(exclusionEntity.Requester);
        actual.Approver.Should().Be(exclusionEntity.Approver);
        actual.ExclusionReasonApprover.Should().Be(exclusionEntity.ExclusionReasonApprover);
        actual.ExclusionReasonRequester.Should().Be(exclusionEntity.ExclusionReasonRequester);
    }

    [Fact]
    public async Task GetExclusionAsync_WhenRequestFailedExceptionIsThrown_ShouldReturnNull()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<int>();
        var pipelineType = _fixture.Create<string>();

        _repositoryMock.Setup(x => x.GetRecordAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Throws(new RequestFailedException((int)HttpStatusCode.NotFound, _fixture.Create<string>()));

        // Act
        var actual = await _sut.GetExclusionAsync(organization, projectId, pipelineId, pipelineType);

        // Assert
        actual.Should().BeNull();
    }

    [Fact]
    public async Task CreateOrUpdateExclusionAsync_With_Should()
    {
        // Arrange
        var exclusion = _fixture.Create<Exclusion>();

        var exclusionEntity = _fixture.Create<ExclusionEntity>();
        _repositoryMock.Setup(x => x.GetRecordAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(exclusionEntity);

        // Act
        var actual = await _sut.CreateOrUpdateExclusionAsync(exclusion);

        // Assert
        actual.Should().NotBeNull();
    }
}
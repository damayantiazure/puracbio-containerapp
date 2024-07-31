#nullable enable

using Rabobank.Compliancy.Functions.Sm9Changes.Application;
using Rabobank.Compliancy.Functions.Sm9Changes.Exceptions;
using Rabobank.Compliancy.Functions.Sm9Changes.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.Sm9Client.Change.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloseChangeRequest = Rabobank.Compliancy.Functions.Sm9Changes.Application.CloseChangeRequest;
using Rabobank.Compliancy.Functions.Sm9Changes.Model;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Tests.Application;

public class CloseChangeProcessTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IChangeService> _changeServiceMock = new();
    private readonly Mock<ISm9ChangesService> _sm9ChangesServiceMock = new();
    private readonly Mock<IAzdoRestClient> _azdoRestClientMock = new();
    private readonly TimeOutSettings _timeOutSettings = new() { TimeOutValue = 5 };

    private readonly CloseChangeProcess _sut;

    public CloseChangeProcessTests()
    {
        _sut = new CloseChangeProcess(
            _changeServiceMock.Object,
            _sm9ChangesServiceMock.Object,
            _azdoRestClientMock.Object,
            _timeOutSettings);
    }

    [Theory]
    [InlineData(SM9Constants.BuildPipelineType)]
    [InlineData(SM9Constants.ReleasePipelineType)]
    public async Task InvalidChangeIdProvided_AndNoTagsOnPipeline_ThrowsChangeIdNotFoundException(string pipelineType)
    {
        // Arrange
        var closeChangeRequest = _fixture.Build<CloseChangeRequest>()
            .With(x => x.PipelineType, pipelineType) // pipelinetype should be valid to get a valid IAzdoService
            .Create();

        // Act
        Func<Task<(IEnumerable<string>, IEnumerable<string>)>> actual = () => _sut.CloseChangeAsync(closeChangeRequest);

        // Assert
        await actual.ShouldThrowAsync<ChangeIdNotFoundException>();
    }

    [Theory]
    [InlineData(SM9Constants.BuildPipelineType, 1, "C000000000", "C000000000 [00000000]", "Random")]
    [InlineData(SM9Constants.BuildPipelineType, 1, "C000000000", "C000000000 [00000000]", "C000000000 [11111111]", "Random")]
    [InlineData(SM9Constants.BuildPipelineType, 2, "C000000000,C111111111", "C000000000 [00000000]", "C111111111 [00000000]", "C222222222")]
    [InlineData(SM9Constants.ReleasePipelineType, 1, "C000000000", "C000000000 [00000000]", "Random")]
    [InlineData(SM9Constants.ReleasePipelineType, 1, "C000000000", "C000000000 [00000000]", "C000000000 [00000000]", "C111111111")]
    [InlineData(SM9Constants.ReleasePipelineType, 3, "C000000000,C111111111,C222222222", "C000000000 [00000000]", "C111111111 [00000000]", "C222222222 [00000000]")]
    public async Task ValidChangeIdViaTags_ClosesChangesAndReturnsIds(
        string pipelineType, int expectedCount, string expectedOutput, params string[] tags)
    {
        // Arrange
        _fixture.Customize<Infra.AzdoClient.Response.Tags>(t => t
            .With(tag => tag.Value, tags));
        var closeChangeRequest = _fixture.Build<CloseChangeRequest>()
            .With(changeRequest => changeRequest.PipelineType, pipelineType) // pipelinetype should be valid to get a valid IAzdoService
            .Create();
        var expectedChangeIds = expectedOutput.Split(",");

        _azdoRestClientMock
            .Setup(m => m.GetAsync(It.IsAny<IAzdoRequest<Infra.AzdoClient.Response.Tags>>(), It.IsAny<string>()))
            .ReturnsAsync(new Infra.AzdoClient.Response.Tags { Value = tags });

        var changeDetails = expectedChangeIds
            .Select(x => _fixture.Build<ChangeInformation>()
                .With(i => i.ChangeId, x)
                .With(i => i.Phase, "Deployment")
                .With(i => i.HasCorrectPhase, true)
                .Create());

        _sm9ChangesServiceMock
            .Setup(m => m.ValidateChangesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<int>()))
            .ReturnsAsync(changeDetails);
            
        // Act
        var (validChangeIds, _) = await _sut.CloseChangeAsync(closeChangeRequest);

        // Assert
        _changeServiceMock
            .Verify(mock => mock.CloseChangesAsync(It.IsAny<CloseChangeDetails>(), It.IsAny<IEnumerable<string>>()), Times.Once);
        _azdoRestClientMock
            .Verify(a => a.GetAsync(It.IsAny<IAzdoRequest<Infra.AzdoClient.Response.Tags>>(), It.IsAny<string>()), Times.Once);

        Assert.Equal(expectedCount, validChangeIds.Count());
        Assert.Equal(expectedChangeIds, validChangeIds);
    }

    [Fact]
    public async Task ValidateChangesAsync_WithInvalidChangePhase_ThrowsChangePhaseValidationException()
    {
        // Arrange
        var closeChangeRequest = new CloseChangeRequest
        {
            PipelineType = SM9Constants.BuildPipelineType,
            CloseChangeDetails = new CloseChangeDetails
            {
                ChangeId = "C123456789"
            }
        };
            
        var changeInfo = _fixture.Build<ChangeInformation>()
            .With(i => i.ChangeId, "C123456789")
            .With(i => i.Phase, "WrongPhase")
            .With(i => i.HasCorrectPhase, false)
            .CreateMany(1);

        _sm9ChangesServiceMock
            .Setup(m => m.ValidateChangesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<int>()))
            .ReturnsAsync(changeInfo);

        // Act
        Func<Task> actual = () => _sut.CloseChangeAsync(closeChangeRequest);

        // Assert
        await actual.ShouldThrowAsync<ChangePhaseValidationException>();
    }
}
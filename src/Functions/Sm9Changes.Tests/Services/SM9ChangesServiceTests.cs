#nullable enable

using Rabobank.Compliancy.Functions.Sm9Changes.Services;
using Rabobank.Compliancy.Infra.Sm9Client.Change;
using Rabobank.Compliancy.Infra.Sm9Client.Change.Model;
using System.Linq;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Tests.Services;

public class SM9ChangesServiceTests
{
    private readonly Sm9ChangesService _sut;
    private readonly Mock<IChangeClient> _changeClientMock = new();
    private IFixture _fixture = new Fixture();
    public SM9ChangesServiceTests()
    {
        _sut = new Sm9ChangesService(_changeClientMock.Object);
    }

    [Theory]
    [InlineData("CorrectPhase", true)]
    [InlineData("InvalidPhase", false)]
    public async Task ValidateChangesAsync_ShouldReturnCorrectChangeInformation(string phase, bool expectedResult)
    {
        // Arrange
        var changeIds = new[] { "C12345678" };
        var correctChangePhases = new[] { "CorrectPhase" };
            
        _fixture.Customize<ChangeInformation>(i => i
            .With(changeInformation => changeInformation.Phase, phase));

        _changeClientMock
            .Setup(changeClient => changeClient.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()))
            .ReturnsAsync(_fixture.Create<GetChangeByKeyResponse>());

        // Act
        var result = await _sut.ValidateChangesAsync(changeIds, correctChangePhases, 5);

        // Assert
        _changeClientMock
            .Verify(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()), Times.Once);
        result.Count().ShouldBe(1);
        result.First().HasCorrectPhase.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task ValidChangeIdViaInput_OnlySecondGetChangeAsyncCallRetrievesCorrectPhase_ShouldReturnCorrectChangeInformation()
    {
        // Arrange
        const int timeout = 20; // This should allow the retry to kick of at least twice, as each wait is 10 seconds
        var changeIds = new[] { "C12345678" };
        var correctChangePhases = new[] { "CorrectPhase" };

        _changeClientMock
            .SetupSequence(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()))
            .ReturnsAsync(CreateChangeByKeyResponse(validResponse: false))
            .ReturnsAsync(CreateChangeByKeyResponse(validResponse: true));
           
        // Act
        var result = await _sut.ValidateChangesAsync(changeIds, correctChangePhases, timeout);

        // Assert
        _changeClientMock
            .Verify(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()), Times.Exactly(2));
        result.Count().ShouldBe(1);
        result.First().HasCorrectPhase.ShouldBeTrue();
    }

    private GetChangeByKeyResponse CreateChangeByKeyResponse(bool validResponse = true) =>
        new()
        {
            Messages = _fixture.CreateMany<string>().ToArray(),
            RetrieveChangeInfoByKey = new ChangeByKey
            {
                Information = new[]
                {
                    new ChangeInformation
                    {
                        Phase = validResponse
                            ? "CorrectPhase"
                            : null
                    }
                }
            },
            ReturnCode = _fixture.Create<string>()
        };
}
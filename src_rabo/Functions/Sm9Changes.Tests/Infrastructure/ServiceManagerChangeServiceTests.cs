#nullable enable

using Rabobank.Compliancy.Functions.Sm9Changes.Application;
using Rabobank.Compliancy.Functions.Sm9Changes.Infrastructure;
using Rabobank.Compliancy.Infra.Sm9Client.Change;
using Rabobank.Compliancy.Infra.Sm9Client.Change.Model;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Tests.Infrastructure;

public class ServiceManagerChangeServiceTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IChangeClient> _changeClientMock = new();
    private readonly IChangeService _sut;

    public ServiceManagerChangeServiceTests() =>
        _sut = new ServiceManagerChangeService(_changeClientMock.Object);

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(60)]
    [InlineData(999)]
    public async Task MultipleChangeIds_ShouldCloseMultipleChanges(int expectedCount)
    {
        // Arrange
        var changeIds = _fixture.CreateMany<string>(expectedCount);
        var requestBody = _fixture.Create<CloseChangeDetails>();

        // Act
        await _sut.CloseChangesAsync(requestBody, changeIds);

        // Assert
        _changeClientMock.Verify(mock => mock.CloseChangeAsync(It.IsAny<CloseChangeRequestBody>()), Times.Exactly(expectedCount));
    }
}
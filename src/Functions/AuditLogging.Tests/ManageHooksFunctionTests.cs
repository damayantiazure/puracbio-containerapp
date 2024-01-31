#nullable enable

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.AuditLogging.Tests;

public class ManageHooksFunctionTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IManageHooksService> _manageHooksService = new();

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public async Task RunAsync_ForEachOrganization_ShouldCallManageHooksService(int organizationCount)
    {
        // Arrange
        var organizations = _fixture.CreateMany<string>(organizationCount).ToArray();
        var sut = new ManageHooksFunction(organizations, _manageHooksService.Object);

        // Act
        await sut.RunAsync(new TimerInfo(null, new ScheduleStatus()));

        // Assert
        _manageHooksService.Verify(c => c.ManageHooksOrganizationAsync(It.IsAny<string>()), Times.Exactly(organizationCount));
    }
}
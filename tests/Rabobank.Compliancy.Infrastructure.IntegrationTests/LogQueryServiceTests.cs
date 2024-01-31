using Rabobank.Compliancy.Infrastructure.Config;
using Rabobank.Compliancy.Infrastructure.Dto.Logging;
using Rabobank.Compliancy.Infrastructure.IntegrationTests.Helpers;

namespace Rabobank.Compliancy.Infrastructure.IntegrationTests;

[Trait("category", "integration")]
public class LogQueryServiceTests
{
    private readonly LogAnalyticsFixture _logAnalyticsFixture = new();

    [Fact]
    public async Task GetQueryEntryAsync_ShouldReturnData()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var actual = await sut.GetQueryEntryAsync<AuditDeploymentLogDto>(
            "audit_deployment_log_CL | top 1 by TimeGenerated");

        // Assert
        actual!.CiName.Should().NotBeNull();
    }

    [Fact]
    public async Task GetQueryEntriesAsync_ShouldReturnData()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var actual = await sut.GetQueryEntriesAsync<AuditDeploymentLogDto>(
            "audit_deployment_log_CL | top 10 by TimeGenerated");

        // Assert
        actual.First().CiName.Should().NotBeNull();
    }

    private LogQueryService CreateSut() => new(
        _logAnalyticsFixture.GetLogsQueryClient(), new LogConfig
        {
            WorkspaceId = _logAnalyticsFixture.Config.WorkspaceId
        });
}
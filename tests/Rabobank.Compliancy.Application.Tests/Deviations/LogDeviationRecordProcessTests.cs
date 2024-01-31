#nullable enable

using Rabobank.Compliancy.Application.Deviations;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;

namespace Rabobank.Compliancy.Application.Tests.Deviations;

public class LogDeviationRecordProcessTests
{
    [Fact]
    public async Task LogDeviationReportRecord_ValidDeviationJsonData_CallsLogService()
    {
        // Arrange        
        var jsonData = await GetStringContentFromAssetsFile("DeviationLogRecord.json");

        var deviationLogServiceMock = new Mock<IDeviationLogService>();
        var logDeviationRecordProcess = new LogDeviationRecordProcess(deviationLogServiceMock.Object);

        // Act
        await logDeviationRecordProcess.LogDeviationReportRecord(jsonData);

        // Assert
        deviationLogServiceMock.Verify(m => m.LogDeviationRecord(It.IsAny<DeviationReportLogRecord>()));
    }

    private static Task<string> GetStringContentFromAssetsFile(string filename) =>
        File.ReadAllTextAsync(Path.Combine(
            "Assets", filename));
}
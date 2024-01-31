using FluentAssertions;
using Rabobank.Compliancy.Application.MonitoringDashboard;
using Rabobank.Compliancy.Application.MonitoringDashboard.Dto;
using Rabobank.Compliancy.Application.Services;

namespace Rabobank.Compliancy.Application.Tests.MonitoringDashboard;

public class MonitoringDashboardTileProcessTests
{
    private const string _purple = "#68217A";
    private const string _orange = "#F2700F";
    private const string _red = "#DA0A00";

    private readonly Mock<IMonitoringDashboardTileService> _monitoringDashboardTileService = new();

    [Theory]
    [InlineData(0, _red)]
    [InlineData(1, _purple)]
    public async Task GetWidgetContentForTile_ForAuditLogging_HasExpectedColor(long numberToReturn, string expectedColor)
    {
        // Arrange
        var informationObject = new AuditLoggingDashboardTileInformation();
        _monitoringDashboardTileService.Setup(m => m.GetMonitoringDashboardDigitByTitle(informationObject.Title, default))
            .ReturnsAsync(numberToReturn);

        // Act
        var actual = await new MonitoringDashboardTileProcess(_monitoringDashboardTileService.Object)
            .GetWidgetContentForTile(informationObject, default);

        // Assert
        actual.Should().Contain($"background-color:{expectedColor}");
        actual.Should().Contain(informationObject.Title);
    }

    [Theory]
    [InlineData(0, _purple)]
    [InlineData(1, _red)]
    [InlineData(100, _red)]
    public async Task GetWidgetContentForTile_ForAuditLoggingError_HasExpectedColor(long numberToReturn, string expectedColor)
    {
        // Arrange
        var informationObject = new AuditLoggingErrorDashboardTileInformation();
        _monitoringDashboardTileService.Setup(m => m.GetMonitoringDashboardDigitByTitle(informationObject.Title, default))
            .ReturnsAsync(numberToReturn);

        // Act
        var actual = await new MonitoringDashboardTileProcess(_monitoringDashboardTileService.Object)
            .GetWidgetContentForTile(informationObject, default);

        // Assert
        actual.Should().Contain($"background-color:{expectedColor}");
        actual.Should().Contain(informationObject.Title);
    }

    [Theory]
    [InlineData(0, _purple)]
    [InlineData(1, _red)]
    [InlineData(100, _red)]
    public async Task GetWidgetContentForTile_ForAuditLoggingPoisonQueue_HasExpectedColor(long numberToReturn, string expectedColor)
    {
        // Arrange
        var informationObject = new AuditLoggingPoisonQueueDashboardTileInformation();
        _monitoringDashboardTileService.Setup(m => m.GetMonitoringDashboardDigitByTitle(informationObject.Title, default))
            .ReturnsAsync(numberToReturn);

        // Act
        var actual = await new MonitoringDashboardTileProcess(_monitoringDashboardTileService.Object)
            .GetWidgetContentForTile(informationObject, default);

        // Assert
        actual.Should().Contain($"background-color:{expectedColor}");
        actual.Should().Contain(informationObject.Title);
    }

    [Theory]
    [InlineData(0, _red)]
    [InlineData(1, _red)]
    [InlineData(99, _red)]
    [InlineData(100, _purple)]
    [InlineData(101, _purple)]
    public async Task GetWidgetContentForTile_ForComplianceScannerItems_HasExpectedColor(long numberToReturn, string expectedColor)
    {
        // Arrange
        var informationObject = new ComplianceScannerItemsDashboardTileInformation();
        _monitoringDashboardTileService.Setup(m => m.GetMonitoringDashboardDigitByTitle(informationObject.Title, default))
            .ReturnsAsync(numberToReturn);

        // Act
        var actual = await new MonitoringDashboardTileProcess(_monitoringDashboardTileService.Object)
            .GetWidgetContentForTile(informationObject, default);

        // Assert
        actual.Should().Contain($"background-color:{expectedColor}");
        actual.Should().Contain(informationObject.Title);
    }

    [Theory]
    [InlineData(0, _purple)]
    [InlineData(1, _orange)]
    [InlineData(9, _orange)]
    [InlineData(10, _red)]
    [InlineData(11, _red)]
    public async Task GetWidgetContentForTile_ForComplianceScannerOnlineError_HasExpectedColor(long numberToReturn, string expectedColor)
    {
        // Arrange
        var informationObject = new ComplianceScannerOnlineErrorDashboardTileInformation();
        _monitoringDashboardTileService.Setup(m => m.GetMonitoringDashboardDigitByTitle(informationObject.Title, default))
            .ReturnsAsync(numberToReturn);

        // Act
        var actual = await new MonitoringDashboardTileProcess(_monitoringDashboardTileService.Object)
            .GetWidgetContentForTile(informationObject, default);

        // Assert
        actual.Should().Contain($"background-color:{expectedColor}");
        actual.Should().Contain(informationObject.Title);
    }

    [Theory]
    [InlineData(0, _red)]
    [InlineData(1, _red)]
    [InlineData(99, _red)]
    [InlineData(100, _purple)]
    [InlineData(101, _purple)]
    public async Task GetWidgetContentForTile_ForComplianceScannerPrinciples_HasExpectedColor(long numberToReturn, string expectedColor)
    {
        // Arrange
        var informationObject = new ComplianceScannerPrinciplesDashboardTileInformation();
        _monitoringDashboardTileService.Setup(m => m.GetMonitoringDashboardDigitByTitle(informationObject.Title, default))
            .ReturnsAsync(numberToReturn);

        // Act
        var actual = await new MonitoringDashboardTileProcess(_monitoringDashboardTileService.Object)
            .GetWidgetContentForTile(informationObject, default);

        // Assert
        actual.Should().Contain($"background-color:{expectedColor}");
        actual.Should().Contain(informationObject.Title);
    }

    [Theory]
    [InlineData(0, _red)]
    [InlineData(1, _red)]
    [InlineData(99, _red)]
    [InlineData(100, _purple)]
    [InlineData(101, _purple)]
    public async Task GetWidgetContentForTile_ForComplianceScannerRules_HasExpectedColor(long numberToReturn, string expectedColor)
    {
        // Arrange
        var informationObject = new ComplianceScannerRulesDashboardTileInformation();
        _monitoringDashboardTileService.Setup(m => m.GetMonitoringDashboardDigitByTitle(informationObject.Title, default))
            .ReturnsAsync(numberToReturn);

        // Act
        var actual = await new MonitoringDashboardTileProcess(_monitoringDashboardTileService.Object)
            .GetWidgetContentForTile(informationObject, default);

        // Assert
        actual.Should().Contain($"background-color:{expectedColor}");
        actual.Should().Contain(informationObject.Title);
    }

    [Theory]
    [InlineData(0, _red)]
    [InlineData(1, _red)]
    [InlineData(99, _red)]
    [InlineData(100, _purple)]
    [InlineData(101, _purple)]
    public async Task GetWidgetContentForTile_ForCompliancyScannerCis_HasExpectedColor(long numberToReturn, string expectedColor)
    {
        // Arrange
        var informationObject = new CompliancyScannerCisDashboardTileInformation();
        _monitoringDashboardTileService.Setup(m => m.GetMonitoringDashboardDigitByTitle(informationObject.Title, default))
            .ReturnsAsync(numberToReturn);

        // Act
        var actual = await new MonitoringDashboardTileProcess(_monitoringDashboardTileService.Object)
            .GetWidgetContentForTile(informationObject, default);

        // Assert
        actual.Should().Contain($"background-color:{expectedColor}");
        actual.Should().Contain(informationObject.Title);
    }

    [Theory]
    [InlineData(0, _purple)]
    [InlineData(1, _orange)]
    [InlineData(9, _orange)]
    [InlineData(10, _red)]
    [InlineData(11, _red)]
    public async Task GetWidgetContentForTile_ForErrorHandling_HasExpectedColor(long numberToReturn, string expectedColor)
    {
        // Arrange
        var informationObject = new ErrorHandlingDashboardTileInformation();
        _monitoringDashboardTileService.Setup(m => m.GetMonitoringDashboardDigitByTitle(informationObject.Title, default))
            .ReturnsAsync(numberToReturn);

        // Act
        var actual = await new MonitoringDashboardTileProcess(_monitoringDashboardTileService.Object)
            .GetWidgetContentForTile(informationObject, default);

        // Assert
        actual.Should().Contain($"background-color:{expectedColor}");
        actual.Should().Contain(informationObject.Title);
    }

    [Theory]
    [InlineData(0, _purple)]
    [InlineData(1, _red)]
    [InlineData(100, _red)]
    public async Task GetWidgetContentForTile_ForHooksFailures_HasExpectedColor(long numberToReturn, string expectedColor)
    {
        // Arrange
        var informationObject = new HooksFailuresDashboardTileInformation();
        _monitoringDashboardTileService.Setup(m => m.GetMonitoringDashboardDigitByTitle(informationObject.Title, default))
            .ReturnsAsync(numberToReturn);

        // Act
        var actual = await new MonitoringDashboardTileProcess(_monitoringDashboardTileService.Object)
            .GetWidgetContentForTile(informationObject, default);

        // Assert
        actual.Should().Contain($"background-color:{expectedColor}");
        actual.Should().Contain(informationObject.Title);
    }

    [Theory]
    [InlineData(0, _purple)]
    [InlineData(1, _red)]
    [InlineData(100, _red)]
    public async Task GetWidgetContentForTile_ForPipelineBreakerDecoratorError_HasExpectedColor(long numberToReturn, string expectedColor)
    {
        // Arrange
        var informationObject = new PipelineBreakerDecoratorErrorDashboardTileInformation();
        _monitoringDashboardTileService.Setup(m => m.GetMonitoringDashboardDigitByTitle(informationObject.Title, default))
            .ReturnsAsync(numberToReturn);

        // Act
        var actual = await new MonitoringDashboardTileProcess(_monitoringDashboardTileService.Object)
            .GetWidgetContentForTile(informationObject, default);

        // Assert
        actual.Should().Contain($"background-color:{expectedColor}");
        actual.Should().Contain(informationObject.Title);
    }

    [Theory]
    [InlineData(0, _purple)]
    [InlineData(1, _orange)]
    [InlineData(9, _orange)]
    [InlineData(10, _red)]
    [InlineData(11, _red)]
    public async Task GetWidgetContentForTile_ForPipelineBreakerError_HasExpectedColor(long numberToReturn, string expectedColor)
    {
        // Arrange
        var informationObject = new PipelineBreakerErrorDashboardTileInformation();
        _monitoringDashboardTileService.Setup(m => m.GetMonitoringDashboardDigitByTitle(informationObject.Title, default))
            .ReturnsAsync(numberToReturn);

        // Act
        var actual = await new MonitoringDashboardTileProcess(_monitoringDashboardTileService.Object)
            .GetWidgetContentForTile(informationObject, default);

        // Assert
        actual.Should().Contain($"background-color:{expectedColor}");
        actual.Should().Contain(informationObject.Title);
    }

    [Theory]
    [InlineData(0, _purple)]
    [InlineData(1, _red)]
    [InlineData(100, _red)]
    public async Task GetWidgetContentForTile_ForSm9ChangesError_HasExpectedColor(long numberToReturn, string expectedColor)
    {
        // Arrange
        var informationObject = new Sm9ChangesErrorDashboardTileInformation();
        _monitoringDashboardTileService.Setup(m => m.GetMonitoringDashboardDigitByTitle(informationObject.Title, default))
            .ReturnsAsync(numberToReturn);

        // Act
        var actual = await new MonitoringDashboardTileProcess(_monitoringDashboardTileService.Object)
            .GetWidgetContentForTile(informationObject, default);

        // Assert
        actual.Should().Contain($"background-color:{expectedColor}");
        actual.Should().Contain(informationObject.Title);
    }

    [Theory]
    [InlineData(0, _purple)]
    [InlineData(1, _red)]
    [InlineData(100, _red)]
    public async Task GetWidgetContentForTile_ForValidateGatesError_HasExpectedColor(long numberToReturn, string expectedColor)
    {
        // Arrange
        var informationObject = new ValidateGatesErrorDashboardTileInformation();
        _monitoringDashboardTileService.Setup(m => m.GetMonitoringDashboardDigitByTitle(informationObject.Title, default))
            .ReturnsAsync(numberToReturn);

        // Act
        var actual = await new MonitoringDashboardTileProcess(_monitoringDashboardTileService.Object)
            .GetWidgetContentForTile(informationObject, default);

        // Assert
        actual.Should().Contain($"background-color:{expectedColor}");
        actual.Should().Contain(informationObject.Title);
    }
}
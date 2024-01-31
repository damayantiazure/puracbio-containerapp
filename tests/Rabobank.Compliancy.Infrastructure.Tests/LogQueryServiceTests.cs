using System.Reflection;
using System.Text.Json;
using Azure;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Infrastructure.Config;
using Rabobank.Compliancy.Infrastructure.Dto.Logging;

namespace Rabobank.Compliancy.Infrastructure.Tests;

public class LogQueryServiceTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<LogsQueryClient> _logsQueryClient = new();
    private readonly LogQueryService _sut;

    public LogQueryServiceTests() =>
        _sut = new LogQueryService(_logsQueryClient.Object, _fixture.Create<LogConfig>());

    [Fact]
    public async Task GetQueryEntryAsync_WithCorrectlyFilledProperties_ShouldReturnExpectedResponse()
    {
        // Arrange
        var completedOn = _fixture.Create<DateTime>();
        var ciName = _fixture.Create<string>();
        var runUrl = _fixture.Create<string>();
        var result = CreateLogsQueryResult(completedOn, ciName, runUrl);

        const string kustoQuery = "log_table_1_CL | union log_table_2_CL | summarize count() by Type";
        const string kustoQueryTransformed =
            "union log_table_1*_CL | union log_table_2*_CL | summarize count() by Type";

        var responseMock = new Mock<Response<LogsQueryResult>>();
        responseMock.SetupGet(m => m.Value).Returns(result);
        responseMock.SetupGet(m => m.HasValue).Returns(true);

        _logsQueryClient.Setup(m => m.QueryWorkspaceAsync(
            It.IsAny<string>(),
            kustoQueryTransformed,
            It.IsAny<QueryTimeRange>(),
            It.IsAny<LogsQueryOptions>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(responseMock.Object);

        // Act
        var actual = await _sut.GetQueryEntryAsync<AuditDeploymentLogDto>(kustoQuery);

        // Assert
        actual!.CiName.Should().Be(ciName);
        actual.RunUrl.Should().Be(runUrl);
        actual.CompletedOn.Should().BeSameDateAs(completedOn);
    }

    [Fact]
    public async Task GetQueryEntriesAsync_WithCorrectlyFilledProperties_ShouldReturnExpectedResponse()
    {
        // Arrange
        var completedOn = _fixture.Create<DateTime>();
        var ciName = _fixture.Create<string>();
        var runUrl = _fixture.Create<string>();
        var result = CreateLogsQueryResult(completedOn, ciName, runUrl);

        const string kustoQuery = "log_table_1_CL | union log_table_2_CL | summarize count() by Type";
        const string kustoQueryTransformed =
            "union log_table_1*_CL | union log_table_2*_CL | summarize count() by Type";

        var responseMock = new Mock<Response<LogsQueryResult>>();
        responseMock.SetupGet(m => m.Value).Returns(result);
        responseMock.SetupGet(m => m.HasValue).Returns(true);

        _logsQueryClient.Setup(m => m.QueryWorkspaceAsync(
            It.IsAny<string>(),
            kustoQueryTransformed,
            It.IsAny<QueryTimeRange>(),
            It.IsAny<LogsQueryOptions>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(responseMock.Object);

        // Act
        var actual = (await _sut.GetQueryEntriesAsync<AuditDeploymentLogDto>(kustoQuery)).ToList();

        // Assert
        actual.Single().CiName.Should().Be(ciName);
        actual.Single().RunUrl.Should().Be(runUrl);
        actual.Single().CompletedOn.Should().BeSameDateAs(completedOn);
    }

    [Fact]
    public async Task GetQueryEntriesAsync_WithNoWorkspaceQueryResponse_ShouldThrowItemNotFoundException()
    {
        // Arrange
        var kustoQuery = _fixture.Create<string>();

        var responseMock = new Mock<Response<LogsQueryResult>>();
        responseMock.SetupGet(m => m.HasValue).Returns(false);

        _logsQueryClient.Setup(m => m.QueryWorkspaceAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<QueryTimeRange>(),
            It.IsAny<LogsQueryOptions>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(responseMock.Object);


        // Act
        var actual = () => _sut.GetQueryEntriesAsync<AuditDeploymentLogDto>(kustoQuery);

        // Assert
        await actual.Should().ThrowAsync<SourceItemNotFoundException>();
    }

    private LogsQueryResult CreateLogsQueryResult(DateTimeOffset completedOn, string ciName, string runUrl)
    {
        var columns = new List<LogsTableColumn>
        {
            CreateInstance<LogsTableColumn>("CompletedOn_t", LogsColumnType.Datetime),
            CreateInstance<LogsTableColumn>("CiName_s", LogsColumnType.String),
            CreateInstance<LogsTableColumn>("RunUrl_s", LogsColumnType.String)
        };

        var jsonElement = JsonDocument.Parse(JsonSerializer.Serialize(new[]
        {
            new object[] { completedOn, ciName, runUrl }
        })).RootElement;

        var tables = (object)new[]
        {
            CreateInstance<LogsTable>(
                _fixture.Create<string>(),
                columns, jsonElement)
        };

        return CreateInstance<LogsQueryResult>(tables);
    }

    private static T CreateInstance<T>(params object[] args)
    {
        var type = typeof(T);

        var types = args.Select(a => a.GetType()).ToArray();

        var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, types);

        return ctor == null
            ? throw new InvalidOperationException($"Unable to create instance of {type}")
            : (T)ctor.Invoke(args);
    }
}
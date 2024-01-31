using Azure.Monitor.Query.Models;
using Newtonsoft.Json;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Monitoring;
using Rabobank.Compliancy.Infrastructure.Dto.Logging;
using Rabobank.Compliancy.Infrastructure.Extensions;
using System.Globalization;

namespace Rabobank.Compliancy.Infrastructure.Tests.Extensions;

public class MonitoringExtensionsTests
{
    private const string _dateTimeFormat = "yyyy-MM-ddTHH\\:mm\\:sszzz";

    private readonly BinaryData _emptyBinData = string.Empty
        .ReconstructForIngestion()
        .ToBinaryData();

    private readonly Fixture _fixture = new();

    [Fact]
    public void ToBinaryData_SingleObject_ReturnBinaryData()
    {
        // Arrange
        var creationDate = DateTime.SpecifyKind(TruncateMilliseconds(_fixture.Create<DateTime>()), DateTimeKind.Local);
        var expectedCreationDate = creationDate.ToString(_dateTimeFormat, CultureInfo.InvariantCulture);
        var createdBy = _fixture.Create<string>();
        var approvers = _fixture.CreateMany<string>().ToList();

        var deploymentInfo = new AuditPullRequestApproversLogDto
        {
            CreatedBy = createdBy,
            CreationDate = creationDate,
            Approvers = approvers
        };

        // Act
        var binaryData = deploymentInfo
            .ReconstructForIngestion()
            .ToBinaryData();

        // Assert
        var json = binaryData.ToString();
        json.Should().Contain($"\"CreationDate_t\":\"{expectedCreationDate}\"");
        json.Should().Contain($"\"CreatedBy_s\":\"{createdBy}\"");
        json.Should().Contain($"\"Approvers_s\":[\"{approvers[0]}\"");
    }

    [Fact]
    public void ToBinaryData_ListOfObjects_ReturnBinaryDataOfObjects()
    {
        // Arrange
        var createdByOne = _fixture.Create<string>();
        var creationDateOne =
            DateTime.SpecifyKind(TruncateMilliseconds(_fixture.Create<DateTime>()), DateTimeKind.Local);
        var expectedCreationDateOne = creationDateOne.ToString(_dateTimeFormat, CultureInfo.InvariantCulture);
        var approversOne = _fixture.CreateMany<string>().ToList();

        var createdByTwo = _fixture.Create<string>();
        var creationDateTwo =
            DateTime.SpecifyKind(TruncateMilliseconds(_fixture.Create<DateTime>()), DateTimeKind.Local);
        var expectedCreationDateTwo = creationDateTwo.ToString(_dateTimeFormat, CultureInfo.InvariantCulture);
        var approversTwo = _fixture.CreateMany<string>().ToList();

        var deploymentInfos = new List<AuditPullRequestApproversLogDto>
        {
            new()
            {
                CreatedBy = createdByOne,
                CreationDate = creationDateOne,
                Approvers = approversOne
            },
            new()
            {
                CreatedBy = createdByTwo,
                CreationDate = creationDateTwo,
                Approvers = approversTwo
            }
        };

        // Act
        var binaryData = deploymentInfos
            .ReconstructForIngestion()
            .ToBinaryData();

        // Assert
        var json = binaryData.ToString();

        json.Should().Contain($"\"CreatedBy_s\":\"{createdByOne}\"");
        json.Should().Contain($"\"CreationDate_t\":\"{expectedCreationDateOne}\"");
        json.Should().Contain($"\"Approvers_s\":[\"{approversOne[0]}\"");

        json.Should().Contain($"\"CreatedBy_s\":\"{createdByTwo}\"");
        json.Should().Contain($"\"CreationDate_t\":\"{expectedCreationDateTwo}\"");
        json.Should().Contain($"\"Approvers_s\":[\"{approversTwo[0]}\"");
    }

    [Fact]
    public void ToGenericObject_QueryResultsNull_ReturnsNull()
    {
        // Arrange
        LogsQueryResult? queryResult = null;

        // Act
        var result = queryResult.ToGenericObject<AuditPullRequestApproversLogDto>();

        // Assert
        result.Should().BeNull();
    }



    [Fact]
    public void ToGenericObject_QueryResultsEmpty_ReturnsNull()
    {
        // Arrange
        var emptyTableList = Enumerable.Empty<LogsTable>().ToList();
        var queryResult = MonitorQueryModelFactory.LogsQueryResult(
            emptyTableList, _emptyBinData, _emptyBinData, _emptyBinData);

        // Act
        var result = queryResult.ToGenericObject<AuditPullRequestApproversLogDto>();

        // Asset
        result.Should().BeNull();
    }

    [Fact]
    public void ToGenericObject_QueryResultsWithDateTimeStringAndJson_ShouldReturnObject()
    {
        // Arrange
        var dto = _fixture.Create<AuditPullRequestApproversLogDto>();

        var columns = new List<LogsTableColumn>
        {
            MonitorQueryModelFactory.LogsTableColumn("CreatedBy_s", LogsColumnType.String),
            MonitorQueryModelFactory.LogsTableColumn("CreationDate_t", LogsColumnType.Datetime),
            MonitorQueryModelFactory.LogsTableColumn("Approvers_s", LogsColumnType.String)
        };

        var rows = new List<LogsTableRow>
        {
            MonitorQueryModelFactory.LogsTableRow(columns, new List<object>
                { dto.CreatedBy!, dto.CreationDate, JsonConvert.SerializeObject(dto.Approvers!) })
        };

        var table = MonitorQueryModelFactory.LogsTable("deploymentTable", columns, rows);

        var queryResult = MonitorQueryModelFactory.LogsQueryResult(
            new List<LogsTable> { table }, _emptyBinData,
            _emptyBinData, _emptyBinData);

        // Act
        var result = queryResult.ToGenericObject<AuditPullRequestApproversLogDto>();

        // Assert
        result!.CreatedBy.Should().Be(dto.CreatedBy);
        result.CreationDate.Should().BeCloseTo(dto.CreationDate, TimeSpan.FromSeconds(1));
        result.Approvers.Should().BeEquivalentTo(dto.Approvers);
    }

    [Fact]
    public void ToGenericObject_QueryResultsWithEnum_ShouldReturnObject()
    {
        // Arrange
        var dto = _fixture.Create<PipelineBreakerReport>();

        var columns = new List<LogsTableColumn>
        {
            MonitorQueryModelFactory.LogsTableColumn("Result_d", LogsColumnType.Real)
        };

        var rows = new List<LogsTableRow>
        {
            MonitorQueryModelFactory.LogsTableRow(columns, new List<object> { dto.Result })
        };

        var table = MonitorQueryModelFactory.LogsTable("deploymentTable", columns, rows);

        var queryResult = MonitorQueryModelFactory.LogsQueryResult(
            new List<LogsTable> { table }, _emptyBinData,
            _emptyBinData, _emptyBinData);

        // Act
        var result = queryResult.ToGenericObject<PipelineBreakerReport>();

        // Assert
        result!.Result.Should().Be(dto.Result);
    }

    [Fact]
    public void ToGenericObject_QueryResultsWithCount_ShouldReturnObject()
    {
        // Arrange
        var columns = new List<LogsTableColumn>
        {
            MonitorQueryModelFactory.LogsTableColumn("Count", LogsColumnType.Long)
        };

        var resultCount = 10;

        var rows = new List<LogsTableRow>
        {
            MonitorQueryModelFactory.LogsTableRow(columns, new List<object> { resultCount })
        };

        var table = MonitorQueryModelFactory.LogsTable("deploymentTable", columns, rows);

        var queryResult = MonitorQueryModelFactory.LogsQueryResult(
            new List<LogsTable> { table }, _emptyBinData,
            _emptyBinData, _emptyBinData);

        // Act
        var result = queryResult.ToGenericObject<ScalarCountResult>();

        // Assert
        result!.Count.Should().Be(resultCount);
    }

    private static DateTime TruncateMilliseconds(DateTime dateTime) =>
        dateTime.Date.AddHours(dateTime.Hour).AddMinutes(dateTime.Minute).AddSeconds(dateTime.Second);
}
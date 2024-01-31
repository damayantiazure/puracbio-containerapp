using Azure.Identity;
using Azure.Monitor.Query;
using Microsoft.Extensions.Configuration;
using Rabobank.Compliancy.Infrastructure.Config;
using Rabobank.Compliancy.Tests.Helpers;

namespace Rabobank.Compliancy.Infrastructure.IntegrationTests.Helpers;

public class LogAnalyticsFixture
{
    private readonly LogsQueryClient _logsQueryClient;

    public LogAnalyticsFixture()
    {
        var configuration = ConfigurationHelper.ConfigureDefaultFiles();
        var logWriterSection = configuration.GetSection("logging");
        Config = logWriterSection.Get<LogConfig>();

        _logsQueryClient = new LogsQueryClient(new DefaultAzureCredential());
    }

    public LogConfig Config { get; }

    public LogsQueryClient GetLogsQueryClient() => _logsQueryClient;
}
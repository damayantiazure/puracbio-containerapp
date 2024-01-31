using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace Rabobank.Compliancy.Tests.Helpers;

public static class ConfigurationHelper
{
    private const string _environmentVariableNotFoundError =
        "Please provide a valid value for environment variable {0}";

    public static IConfiguration ConfigureDefaultFiles()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile("local.settings.json", true)
            .AddJsonFile("logsettings.development.json", true)
            .Build();

        foreach (var configurationItem in configuration.GetChildren())
        {
            Environment.SetEnvironmentVariable(configurationItem.Key, configurationItem.Value);
        }

        return configuration;
    }

    public static string GetEnvironmentVariable(string variableName) =>
        Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process)
        ?? throw new ArgumentNullException(variableName,
            string.Format(CultureInfo.InvariantCulture, _environmentVariableNotFoundError, variableName));
}
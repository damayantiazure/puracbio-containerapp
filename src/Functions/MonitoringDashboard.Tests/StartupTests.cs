#nullable enable

using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Rabobank.Compliancy.Functions.MonitoringDashboard.Tests;

public class StartupTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void TestDependencyInjectionResolve()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var builder = new Mock<IFunctionsHostBuilder>();
        builder
            .SetupGet(hostBuilder => hostBuilder.Services)
            .Returns(services);

        var functions = typeof(Startup)
            .Assembly
            .GetTypes()
            .Where(type => Array.Exists(type.GetMethods(), method =>
                method.GetCustomAttributes(typeof(FunctionNameAttribute), false).Any() &&
                !method.IsStatic))
            .ToList();

        functions.ForEach(serviceType => services.AddScoped(serviceType));

        Startup.RegisterServices(services, CreateConfig());
        var provider = services.BuildServiceProvider();

        // Act
        var actual = () => functions.ForEach(function => provider.GetService(function));

        // Assert
        actual.Should().NotThrow();
    }

    private IConfiguration CreateConfig()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("logsettings.development.json");

        Environment.SetEnvironmentVariable("azdoPat", _fixture.Create<string>());
        Environment.SetEnvironmentVariable("extensionSecret", _fixture.Create<string>());
        Environment.SetEnvironmentVariable("extensionName", _fixture.Create<string>());
        Environment.SetEnvironmentVariable("validateGatesHostName", _fixture.Create<string>());
        Environment.SetEnvironmentVariable("functionAppHostName", _fixture.Create<string>());
        Environment.SetEnvironmentVariable("globalManagedIdentityClientId", _fixture.Create<string>());
        Environment.SetEnvironmentVariable("itsmEndpointKong", "http://localhost");
        Environment.SetEnvironmentVariable("itsmApiResourceKong", $"{_fixture.Create<Guid>()}");
        Environment.SetEnvironmentVariable("tableStorageConnectionString", "UseDevelopmentStorage=true");
        Environment.SetEnvironmentVariable("eventQueueStorageConnectionString", "UseDevelopmentStorage=true");
        Environment.SetEnvironmentVariable("auditLoggingEventQueueStorageConnectionString", "UseDevelopmentStorage=true");

        return configBuilder.Build();
    }
}
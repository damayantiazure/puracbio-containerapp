#nullable enable

using AutoFixture.AutoMoq;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Rabobank.Compliancy.Functions.AuditLogging.Tests;

public class StartupTests
{

    public StartupTests()
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization());
        Environment.SetEnvironmentVariable("azdoPat", fixture.Create<string>());
        Environment.SetEnvironmentVariable("azdoOrganizations", fixture.Create<string>());
        Environment.SetEnvironmentVariable("extensionName", fixture.Create<string>());
        Environment.SetEnvironmentVariable("tableStorageConnectionString", "UseDevelopmentStorage=true");
        Environment.SetEnvironmentVariable("eventQueueStorageConnectionString", "UseDevelopmentStorage=true");
    }

    [Fact]
    public void TestDependencyInjectionResolve()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var builder = new Mock<IFunctionsHostBuilder>();
        builder
            .Setup(hostBuilder => hostBuilder.Services)
            .Returns(services);

        var functions = typeof(Startup)
            .Assembly
            .GetTypes()
            .Where(type => Array.Exists(type.GetMethods(), method =>
                method.GetCustomAttributes(typeof(FunctionNameAttribute), false).Any() &&
                !method.IsStatic))
            .ToList();

        functions.ForEach(function => services.AddScoped(function));

        Startup.RegisterServices(services, CreateConfig());
        var provider = services.BuildServiceProvider();

        // Act
        var actual = () => functions.ForEach(function => provider.GetService(function));

        // Assert
        actual.Should().NotThrow();
    }

    private static IConfiguration CreateConfig()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("logsettings.development.json");
        return configBuilder.Build();
    }
}
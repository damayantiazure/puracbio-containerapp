#nullable enable

using System;
using System.Linq;
using AutoFixture.AutoMoq;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Rabobank.Compliancy.Functions.PipelineBreaker.Tests;

public class StartupTests
{
    private readonly IFixture _fixture = new Fixture();

    public StartupTests()
    {
        _fixture.Customize(new AutoMoqCustomization());

        Environment.SetEnvironmentVariable("azdoPat", _fixture.Create<string>());
        Environment.SetEnvironmentVariable("extensionName", _fixture.Create<string>());
        Environment.SetEnvironmentVariable("tableStorageConnectionString", "UseDevelopmentStorage=true");
        Environment.SetEnvironmentVariable("blockUnregisteredPipelinesEnabled", _fixture.Create<string>());
        Environment.SetEnvironmentVariable("blockIncompliantPipelinesEnabled", _fixture.Create<string>());
        Environment.SetEnvironmentVariable("throwWarningsIncompliantPipelinesEnabled", _fixture.Create<string>());
        Environment.SetEnvironmentVariable("validateGatesHostName", _fixture.Create<string>());
        Environment.SetEnvironmentVariable("extensionSecret", _fixture.Create<string>());
    }

    [Fact]
    public void TestDependencyInjectionResolve()
    {
        // Arrange
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

    private static IConfiguration CreateConfig()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("logsettings.development.json");
        return configBuilder.Build();
    }
}
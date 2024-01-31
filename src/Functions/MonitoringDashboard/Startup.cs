#nullable enable

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rabobank.Compliancy.Application;
using Rabobank.Compliancy.Functions.MonitoringDashboard;
using Rabobank.Compliancy.Infrastructure;
using System.IO;
using System.Net.Http;


[assembly: FunctionsStartup(typeof(Startup))]

namespace Rabobank.Compliancy.Functions.MonitoringDashboard;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddLogging();

        RegisterServices(builder.Services, builder.GetContext().Configuration);
    }

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        var context = builder.GetContext();
        var environmentName = context.EnvironmentName.ToLower();

        builder.ConfigurationBuilder
            .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"logsettings.{environmentName}.json"), true, false);
    }

    internal static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton(new HttpClient())
            .AddCommonInfraDependencies(configuration)
            .AddMonitoringDashboardTileProcessDependencies()
            .AddMonitoringDashboardTileServiceDependencies();
    }
}
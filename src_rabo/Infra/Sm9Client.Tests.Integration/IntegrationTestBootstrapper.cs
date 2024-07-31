using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Hosting;
using Rabobank.Compliancy.Functions.Sm9Changes;
using Rabobank.Compliancy.Tests.Helpers;

namespace Rabobank.Compliancy.Infra.Sm9Client.Tests.Integration;

public class IntegrationTestBootstrapper
{
    public IntegrationTestBootstrapper()
    {
        var config = ConfigurationHelper.ConfigureDefaultFiles();
        

        HostBuilder = new HostBuilder()
            .ConfigureWebJobs((context, builder) => new Startup().Configure(new WebJobsBuilderContext
            {
                ApplicationRootPath = context.HostingEnvironment.ContentRootPath,
                Configuration = config,
                EnvironmentName = context.HostingEnvironment.EnvironmentName
            }, builder))
            .Build();
    }

    public IHost HostBuilder { get; private set; }
}
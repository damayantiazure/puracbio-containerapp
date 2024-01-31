


using ComplianceWebApi;
using ComplianceWebApi.Configurations;
using ComplianceWebApi.Middleware;
using ComplianceWebApi.Services;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton<PipelineBreakerConfig>();
builder.Services.AddSingleton(
    new AzureDevOpsClientConfig(
        orgName: "raboweb", 
        useManagedIdentity: false,
        clientIdOfManagedIdentity: "",
        tenantIdOfManagedIdentity: "",
        useServicePrincipal: false,
        clientIdOfServicePrincipal: "",
        clientSecretOfServicePrincipal: "",
        tenantIdOfServicePrincipal: "",
        usePat: true,
        Pat: string.Empty));

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IPipelineBreakerService, PipelineBreakerService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.SingleLine = true;
    options.TimestampFormat = "hh:mm:ss ";
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.MapApiRoutes();
app.UseDevOpsAccessTokenValidation();

app.Run();
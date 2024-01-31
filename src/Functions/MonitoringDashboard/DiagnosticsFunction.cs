using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Rabobank.Compliancy.Functions.MonitoringDashboard;

public class DiagnosticsFunction
{
    [FunctionName(nameof(DiagnosticsFunction))]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, Route = "diagnostics")]
        HttpRequestMessage request) =>
        new OkObjectResult(GetVersionEnvironmentVariable());

    private static string GetVersionEnvironmentVariable() =>
        Environment.GetEnvironmentVariable("version", EnvironmentVariableTarget.Process)
        ?? "1.0.-1";
}
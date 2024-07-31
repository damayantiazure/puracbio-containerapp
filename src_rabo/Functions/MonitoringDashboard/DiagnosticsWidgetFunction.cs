using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Web;
using Rabobank.Compliancy.Application.Helpers;

namespace Rabobank.Compliancy.Functions.MonitoringDashboard;

public class DiagnosticsWidgetFunction
{
    private readonly HttpClient _client;

    public DiagnosticsWidgetFunction(HttpClient client) =>
        _client = client;

    [FunctionName(nameof(DiagnosticsWidgetFunction))]
    public Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, Route = "diagnostics-widget")]
        HttpRequestMessage request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var query = request.RequestUri?.Query != null
            ? HttpUtility.ParseQueryString(request.RequestUri.Query)
            : null;

        var functionAppName = query?.Get("functionAppName");

        if (functionAppName == null)
        {
            throw new ArgumentException($"{nameof(functionAppName)} cannot be null.");
        }

        var title = query.Get("title") ?? functionAppName;

        return RunAsyncInternal(title, functionAppName);
    }

    private async Task<IActionResult> RunAsyncInternal(string title, string functionAppName)
    {
        var url = $"https://{functionAppName}.azurewebsites.net/api/diagnostics";

        using var res = await _client.GetAsync(url);
        using var content = res.Content;

        var version = res.IsSuccessStatusCode ? await content.ReadAsStringAsync() : "-";

        return new ContentResult
        {
            Content = WidgetFactory.CreateDiagnosticsWidgetContent(title, res.IsSuccessStatusCode, version),
            ContentType = "text/html"
        };
    }
}
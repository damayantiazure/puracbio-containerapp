


using Compliancy.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Compliancy.PipelineValidationApi;

public static class ApiRouter
{
    public static WebApplication MapApiRoutes(this WebApplication app)
    {
        var apiGroup = app.MapGroup("api");
        var compliancyCheckApiRoute = apiGroup.MapGroup("compliancy").WithOpenApi();

        _ = compliancyCheckApiRoute.MapPost("check", CheckPipelineCompliancyAsync);

        return app;
    }

    private async static Task<ActionResult> CheckPipelineCompliancyAsync(
        [FromBody] object requestBody,
        HttpContext context)
    {
        var taskProperties = GetTaskProperties(context.Request.Headers);

        var executionEngine = new TaskExecution();
        _ = Task.Run(() => executionEngine.ExecuteAsync(taskProperties, new CancellationToken()));

        await Task.CompletedTask;
        return new OkObjectResult("Request accepted!");
    }

    private static TaskProperties GetTaskProperties(IHeaderDictionary requestHeaders)
    {
        IDictionary<string, string> taskProperties = new Dictionary<string, string>();

        foreach (var requestHeader in requestHeaders)
        {
            if(requestHeader.Value.Count != 0)
            {
                var value = requestHeader.Value.FirstOrDefault();
                if(!string.IsNullOrWhiteSpace(value))
                {
                    taskProperties.Add(requestHeader.Key, value);
                }                
            }            
        }

        return new TaskProperties(taskProperties);
    }
}

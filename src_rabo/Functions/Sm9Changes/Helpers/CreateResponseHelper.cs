using Microsoft.AspNetCore.Mvc;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Helpers;

public static class CreateResponseHelper
{
    public static IActionResult CreateResponseTask(string message, bool isSuccess) => 
        isSuccess
            ? new OkObjectResult(message)
            : new BadRequestObjectResult(new { Message = message });

    public static IActionResult CreateResponseGate(string message, bool isSuccess) =>
        isSuccess
            ? new OkObjectResult(message)
            : new BadRequestObjectResult(message);
}
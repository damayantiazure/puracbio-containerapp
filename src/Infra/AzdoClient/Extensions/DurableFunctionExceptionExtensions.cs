using Flurl.Http;
using Rabobank.Compliancy.Infra.AzdoClient.Exceptions;
using System;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Infra.AzdoClient.Extensions;

public static class DurableFunctionExceptionExtensions
{
    public static async Task<Exception> MakeDurableFunctionCompatible(this FlurlHttpException exception)
    {
        var unwrappedExceptionMessage = await exception.GetResponseStringAsync();
        var message = unwrappedExceptionMessage == null
            ? exception.Message 
            : $"{exception.Message} {unwrappedExceptionMessage}";
        var innerMessage = $"{exception.Message}, Original {nameof(FlurlHttpException)} Stacktrace: {exception.StackTrace}";
        var innerException = new Exception(innerMessage, exception.InnerException);

        return message.Contains("\"typeKey\":\"OrchestrationSessionNotFoundException\"")
            ? new OrchestrationSessionNotFoundException(message, innerException)
            : new Exception(message, innerException);
    }
}
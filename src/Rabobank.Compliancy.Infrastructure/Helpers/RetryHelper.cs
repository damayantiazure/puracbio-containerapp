#nullable enable

using System.Net;
using Flurl.Http;
using Polly;

namespace Rabobank.Compliancy.Infrastructure.Helpers;

public static class RetryHelper
{
    private const int MaxNumberOfAttempts = 3; // Maximum of 3 attempts

    public static Task ExecuteBadRequestPolicyAsync(Func<Task> action) =>
        Policy.Handle<FlurlHttpException>(ex =>
                ex.Call.HttpStatus == HttpStatusCode.BadRequest)
            .WaitAndRetryAsync(MaxNumberOfAttempts, _ => TimeSpan.FromSeconds(new Random().Next(5, 20)))
            .ExecuteAsync(action);
}

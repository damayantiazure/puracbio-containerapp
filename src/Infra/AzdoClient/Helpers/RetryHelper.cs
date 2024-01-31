using Flurl.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Polly;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Infra.AzdoClient.Helpers;

public static class RetryHelper
{
    private const int FirstRetryInterval = 10; // First retry happens after 10 secs
    private const int MaxNumberOfAttempts = 3; // Maximum of 3 attempts
    private const double BackoffCoefficient = 1.25; // Back-off timer is multiplied by this number for each retry
    private const int MaxRetryInterval = 5 * 60; // Maximum time to wait
    private const int RetryTimeout = 5 * 60; // Time to wait before a single retry times out

    public static Task ExecuteInvalidDocumentVersionPolicyAsync(string organization, Func<Task> action)
    {
        var invalidDocumentVersionPolicy = Policy
            .Handle<FlurlHttpException>(ex =>
                ex.Call.HttpStatus == HttpStatusCode.BadRequest && ex.Call.Request.IsExtMgtRequest(organization))
            .WaitAndRetryAsync(MaxNumberOfAttempts, retryAttempt => TimeSpan.FromSeconds(new Random().Next(5, 20)));

        return invalidDocumentVersionPolicy.ExecuteAsync(action);
    }

    public static RetryOptions ActivityRetryOptions => new RetryOptions(
        firstRetryInterval: TimeSpan.FromSeconds(FirstRetryInterval),
        maxNumberOfAttempts: MaxNumberOfAttempts)
    {
        BackoffCoefficient = BackoffCoefficient,
        Handle = IsRetryableActivity,
        MaxRetryInterval = TimeSpan.FromSeconds(MaxRetryInterval),
        RetryTimeout = TimeSpan.FromSeconds(RetryTimeout)
    };

    private static bool IsRetryableActivity(Exception exception) =>
        exception.InnerException is TaskCanceledException ||
        exception.InnerException is FunctionTimeoutException;
}
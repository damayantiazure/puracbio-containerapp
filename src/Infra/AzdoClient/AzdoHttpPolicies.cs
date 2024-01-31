using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Flurl.Http;
using Polly;
using Polly.Retry;

namespace Rabobank.Compliancy.Infra.AzdoClient;

public static class AzdoHttpPolicies
{
    const int RetryCount = 3;
    const int NumberToRaisePower = 2;
    const int WaitMultiplyFactor = 1000;

    private static readonly HttpStatusCode[] HttpStatusCodesWorthRetrying =
    {
        // Unauthorized is added to the retry policy because sometimes the AzureDevOps API returns a HTTP401 when having hickups/issues.
        // In the case a real HTTP 401 is occuring, also a retry will be done, but we take that for granted (call will be done max 5 times).
        HttpStatusCode.Unauthorized, // 401
        HttpStatusCode.RequestTimeout, // 408
        HttpStatusCode.FailedDependency, //424
        HttpStatusCode.TooManyRequests, // 429
        HttpStatusCode.BadGateway, // 502
        HttpStatusCode.ServiceUnavailable, // 503
        HttpStatusCode.GatewayTimeout, // 504
        HttpStatusCode.InternalServerError // 500
    };

    private static readonly PolicyBuilder retryPolicyBuilder = Policy
        .Handle<FlurlHttpException>(r => r.Call?.Response?.StatusCode != null && HttpStatusCodesWorthRetrying.Contains(r.Call.Response.StatusCode))
        .Or<SocketException>()
        .OrInner<SocketException>()
        .Or<FlurlHttpTimeoutException>()
        .OrInner<FlurlHttpTimeoutException>()
        .Or<TaskCanceledException>()
        .OrInner<TaskCanceledException>();

    public static AsyncRetryPolicy GetRetryPolicyAsync() => retryPolicyBuilder.WaitAndRetryAsync(RetryCount, i => sleepDurationProvider(i));
    public static RetryPolicy GetRetryPolicy() => retryPolicyBuilder.WaitAndRetry(RetryCount, i => sleepDurationProvider(i));

    private static readonly Func<int, TimeSpan> sleepDurationProvider = (int retryAttempt) =>
        TimeSpan.FromMilliseconds(Math.Pow(retryAttempt, NumberToRaisePower) * WaitMultiplyFactor);
}
using System;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;

namespace FeatureFlag.API.PolyConfig;

public static class PolyConfig
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Handles HttpRequestException, 5xx, and 408 status codes
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // Handle rate limiting (429)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff: 2, 4, 8 seconds
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Optional: Log retry attempts
                    Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                });
    }


    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, timespan) =>
                {
                    // Optional: Log circuit breaker opening
                    Console.WriteLine($"Circuit breaker opened for {timespan.TotalSeconds} seconds due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                },
                onReset: () =>
                {
                    // Optional: Log circuit breaker reset
                    Console.WriteLine("Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    // Optional: Log circuit breaker half-open state
                    Console.WriteLine("Circuit breaker is half-open");
                });
    }
}


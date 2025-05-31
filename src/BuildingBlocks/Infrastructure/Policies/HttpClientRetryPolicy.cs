using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Serilog;

namespace Infrastructure.Policies;

public static class HttpClientRetryPolicy
{
    public static IHttpClientBuilder UseImmediateHttpRetryPolicy(this IHttpClientBuilder builder, int retryCount = 3)
        => builder.AddPolicyHandler(ConfigureImmediateHttpRetry(retryCount));

    public static IHttpClientBuilder UseLinearHttpRetryPolicy(this IHttpClientBuilder builder, int retryCount = 3)
        => builder.AddPolicyHandler(ConfigureLinearHttpRetry(retryCount));

    public static IHttpClientBuilder UseExponentialHttpRetryPolicy(this IHttpClientBuilder builder, int retryCount = 5)
        => builder.AddPolicyHandler(ConfigureExponentialHttpRetry(retryCount));

    public static IHttpClientBuilder UseCircuitBreakerPolicy(this IHttpClientBuilder builder,
                                                             int eventAllowedBeforeBreaking = 3,
                                                             int fromSeconds = 30) // when system break, wait 30s to call again
        => builder.AddPolicyHandler(ConfigureCircuitBreaker(eventAllowedBeforeBreaking, fromSeconds));

    public static IHttpClientBuilder ConfigureTimeoutPolicy(this IHttpClientBuilder builder, int seconds = 5)
        => builder.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(seconds));

    #region Configure
    private static IAsyncPolicy<HttpResponseMessage> ConfigureImmediateHttpRetry(int retryCount)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>() // configure timeout (5 seconds will throw exception)
            .RetryAsync(retryCount,
                (exception, retryCount, context) =>
                {
                    Log.Error($"Retry {retryCount} of {context.PolicyKey} at" +
                              $"{context.OperationKey}, due to: {exception.Exception.Message}");
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> ConfigureLinearHttpRetry(int retryCount)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(3),
                (exception, retryCount, context) =>
                {
                    Log.Error($"Retry {retryCount} of {context.PolicyKey} at" +
                              $"{context.OperationKey}, due to: {exception.Exception.Message}");
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> ConfigureExponentialHttpRetry(int retryCount)
    {
        // In this case will wait for
        //  2 ^ 1 = 2 seconds then
        //  2 ^ 2 = 4 seconds then
        //  2 ^ 3 = 8 seconds then
        //  2 ^ 4 = 16 seconds then
        //  2 ^ 5 = 32 seconds
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, retryCount, context) =>
                {
                    Log.Error($"Retry {retryCount} of {context.PolicyKey} at" +
                              $"{context.OperationKey}, due to: {exception.Exception.Message}");
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> ConfigureCircuitBreaker(int eventAllowedBeforeBreaking, int fromSeconds)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(eventAllowedBeforeBreaking, TimeSpan.FromSeconds(fromSeconds));
    }
    #endregion
}
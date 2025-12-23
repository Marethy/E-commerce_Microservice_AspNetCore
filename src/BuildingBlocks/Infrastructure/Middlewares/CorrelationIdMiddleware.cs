using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Infrastructure.Middlewares;

/// <summary>
/// Middleware to handle correlation ID for distributed tracing across microservices
/// Essential for AI analytics and debugging
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        // Store in HttpContext for easy access throughout the request pipeline
        context.Items[CorrelationIdHeader] = correlationId;
        
        // Add to response headers for client tracking
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Response.Headers.Append(CorrelationIdHeader, correlationId);
            }
            return Task.CompletedTask;
        });

        // Add to log scope for all logs in this request
        using (_logger.BeginScope(new Dictionary<string, object> 
        { 
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path,
            ["RequestMethod"] = context.Request.Method
        }))
        {
            _logger.LogInformation("Request started: {Method} {Path} [CorrelationId: {CorrelationId}]",
                context.Request.Method, context.Request.Path, correlationId);

            await _next(context);

            _logger.LogInformation("Request completed: {Method} {Path} with status {StatusCode} [CorrelationId: {CorrelationId}]",
                context.Request.Method, context.Request.Path, context.Response.StatusCode, correlationId);
        }
    }

    private string GetOrCreateCorrelationId(HttpContext context)
    {
        // Try to get from request headers
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out StringValues correlationId) 
            && !string.IsNullOrEmpty(correlationId))
        {
            return correlationId;
        }

        // Generate new correlation ID
        return Guid.NewGuid().ToString();
    }
}

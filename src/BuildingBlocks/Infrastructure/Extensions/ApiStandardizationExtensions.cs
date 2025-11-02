using Contracts.Services;
using Infrastructure.Filters;
using Infrastructure.Middlewares;
using Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

/// <summary>
/// Extension methods for standardizing API responses and tracking
/// Minimal setup required - just add to your Program.cs
/// </summary>
public static class ApiStandardizationExtensions
{
    /// <summary>
    /// Add standardization services (Response wrapper, User activity tracking)
    /// Call this in Program.cs: builder.Services.AddApiStandardization();
    /// </summary>
    public static IServiceCollection AddApiStandardization(this IServiceCollection services)
    {
        // Add response wrapper filter
        services.AddControllers(options =>
        {
            options.Filters.Add<ApiResponseWrapperFilter>();
        });

        // Add user activity tracking service
        services.AddScoped<IUserActivityService, UserActivityService>();

        return services;
    }

    /// <summary>
    /// Use standardization middleware (Correlation ID tracking)
    /// Call this in Program.cs: app.UseApiStandardization();
    /// </summary>
    public static IApplicationBuilder UseApiStandardization(this IApplicationBuilder app)
    {
        // Add correlation ID middleware (should be early in pipeline)
        app.UseMiddleware<CorrelationIdMiddleware>();

        return app;
    }
}

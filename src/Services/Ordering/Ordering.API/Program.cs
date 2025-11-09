using Common.Logging;
using Contracts.Common.Interfaces;
using Contracts.Messages;
using HealthChecks.UI.Client;
using Infrastructure.Common;
using Infrastructure.Messages;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Ordering.API.Extensions;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(SeriLogger.Configure);
Log.Information($"Starting {builder.Environment.ApplicationName}");

try
{
    ConfigureServices(builder);

    builder.AddAppConfigurations();

    var app = builder.Build();

    if (!Environment.CommandLine.Contains("ef"))
    {
        await ConfigureMiddleware(app);
        await app.RunAsync();
    }
}
catch (Exception ex) when (ex.GetType().Name != "StopTheHostException")
{
    Console.Write(ex.Message);
    Log.Fatal(ex, $"Unhandled exception: {ex.Message}");
}
finally
{
    Log.Information($"Stopping {builder.Environment.ApplicationName}");
    Log.CloseAndFlush();
}

/// <summary>
/// Đăng ký các services vào DI container
/// </summary>
static void ConfigureServices(WebApplicationBuilder builder)
{
    var services = builder.Services;
    var configuration = builder.Configuration;

    services.AddApplicationServices();
    services.AddInfrastructure(configuration);
    services
        .AddConfigurationSettings(configuration)
        .ConfigureMassTransit(configuration);

    services.ConfigureHealthChecks(configuration);
    services.ConfigureOrderingServices();

    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();


}

/// <summary>
/// Cấu hình middleware và seed database
/// </summary>
static async Task ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger Order API v1"));
    }
    app.UseRouting();
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();

        endpoints.MapHealthChecks("/hc", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        endpoints.MapDefaultControllerRoute();
    });
    // Seed database
    await app.SeedOrderDataAsync();
}
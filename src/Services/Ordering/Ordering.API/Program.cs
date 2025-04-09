using Common.Logging;
using Contracts.Common.Interfaces;
using Contracts.Messages;
using Infrastructure.Common;
using Infrastructure.Messages;
using Microsoft.AspNetCore.Builder;
using Ordering.API.Extensions;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Serilog
builder.Host.UseSerilog(SeriLogger.Configure);
Log.Information($"Starting {builder.Environment.ApplicationName}");

try
{
    ConfigureServices(builder);
    builder.AddAppConfigurations();

    var app = builder.Build();

    await ConfigureMiddleware(app);
    await app.RunAsync();
}
catch (Exception ex) when (ex.GetType().Name != "StopTheHostException")
{
    Log.Fatal(ex, $"Unhandled exception: {ex.Message}");
    Console.WriteLine($"Unhandled exception: {ex.Message}");

}
finally
{
    Log.Information($"Stopping {builder.Environment.ApplicationName}");
    Console.WriteLine($"Stopping {builder.Environment.ApplicationName}");
    Log.CloseAndFlush();
}

/// <summary>
/// Đăng ký các services vào DI container
/// </summary>
static void ConfigureServices(WebApplicationBuilder builder)
{
    var services = builder.Services;
    services.AddEmailSettings(builder.Configuration);
    services.AddApplicationServices(); 
    services.AddInfrastructure(builder.Configuration);
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddScoped<IMessageProducer, RabbitMQProducer>();  
    services.AddScoped<ISerializeService, SerializeService>();
    services
    .AddConfigurationSettings(builder.Configuration)
    .ConfiguraMassTransit(builder.Configuration);

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

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    // Seed database
    await app.SeedOrderDataAsync();
}

using Hangfire.API.Extensions;
using HealthChecks.UI.Client;
using Infrastructure.ScheduleJobs;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

Log.Information($"Starting {builder.Environment.ApplicationName} up");

try
{

    builder.Host.AddAppConfigurations();
    builder.Services.AddConfigurationSettings(builder.Configuration);
    builder.Services.ConfigureMongoDbClient();
    builder.Services.AddHangfireService();
    builder.Services.ConfigureServices();
    builder.Services.ConfigureHealthChecks();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();
    app.UseHttpsRedirection();
    app.UseAuthorization();

    app.UseHangfireDashboard(builder.Configuration);
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapHealthChecks("/hc", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpoints.MapDefaultControllerRoute();
    });

    app.Run();
}
catch (Exception ex)
{
    if (ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
        throw;

    Log.Fatal(ex, $"Unhandled exception: {ex.Message}");
}
finally
{
    Log.Information($"Shutdown {builder.Environment.ApplicationName} complete");
    Log.CloseAndFlush();
}
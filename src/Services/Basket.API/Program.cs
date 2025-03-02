using Serilog;
using Common.Logging;
using Basket.API.Extensions;
using Basket.API.Persistence;

var builder = WebApplication.CreateBuilder(args);

Log.Information($"Starting {builder.Environment.ApplicationName}");
try
{
    builder.Host.UseSerilog(SeriLogger.Configure);
    builder.AddAppConfigurations();
    builder.Services.AddInfrastructure(builder.Configuration);

    try
    {
        var app = builder.Build();
        Log.Information($"Environment: {app.Environment.EnvironmentName}");

        app.UseInfrastructure();

        // Migrate database and seed data if needed
        // app.MigrateDatabase<ProductContext>((context, services) =>
        // {
        //     var logger = services.GetService<ILogger<ProductContextSeed>>();
        //     ProductContextSeed.SeedProductAsync(context, logger).Wait();
        // }).Run();

        Log.Information("Application is starting...");

        app.Run(); // Start the application
        Log.Information("Application has stopped.");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, $"❌ Error during application startup: {ex.Message}");
        throw;
    }
}
catch (Exception ex)
{
    if (ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }
    Log.Fatal(ex, $"Unhandled exception: {ex.Message}");
}
finally
{
    Log.Information($"Stopping {builder.Environment.ApplicationName}");
    Log.CloseAndFlush();
}

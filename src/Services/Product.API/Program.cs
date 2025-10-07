using Common.Logging;
using Product.API.Extensions;
using Product.API.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Information($"Starting {builder.Environment.ApplicationName}");
try
{
    builder.Host.UseSerilog(SeriLogger.Configure);
    builder.AddAppConfigurations();

    builder.Services.AddInfrastructure(builder.Configuration);
    
    var app = builder.Build();
    app.UseInfrastructure();
    
    // Try to migrate database and seed data with better error handling
    try
    {
        Log.Information("Attempting database migration and seeding...");
        app.MigrateDatabase<ProductContext>((context, services) =>
        {
            var logger = services.GetService<ILogger<ProductContextSeed>>();
            ProductContextSeed.SeedProductAsync(context, logger).Wait();
        });
        Log.Information("Database migration and seeding completed successfully.");
    }
    catch (Exception dbEx)
    {
        Log.Fatal(dbEx, "Database migration failed: {ErrorMessage}", dbEx.Message);
        throw;
    }
    
    app.Run();
}
catch (Exception ex)
{
    string type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        Log.Information("Host was stopped gracefully.");
        throw;
    }
    Log.Fatal(ex, $"Unhandled exception: {ex.Message}");
}
finally
{
    Log.Information($"Stopping {builder.Environment.ApplicationName}");
    Log.CloseAndFlush();
}
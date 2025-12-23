using Common.Logging;
using Product.API.Extensions;
using Product.API.Persistence;
using Product.API.Commands;
using Serilog;
using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

Log.Information($"Starting {builder.Environment.ApplicationName}");

// Check for command-line arguments
var seedData = args.Contains("--seed-data");
var dataPath = args.SkipWhile(a => a != "--data-path").Skip(1).FirstOrDefault() 
    ?? @"c:\Users\PC\Desktop\source\clean";

try
{
    builder.Host.UseSerilog(SeriLogger.Configure);
    builder.AddAppConfigurations();

    builder.Services.AddInfrastructure(builder.Configuration);
    
    var app = builder.Build();

    // If seed-data flag is present, run seeding and exit
    if (seedData)
    {
        Log.Information("Seeding mode activated");
        await SeedDataCommand.ExecuteAsync(app.Services, dataPath);
        Log.Information("Seeding completed. Exiting application.");
        return 0;
    }

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
    return 0;
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
    return 1;
}
finally
{
    Log.Information($"Stopping {builder.Environment.ApplicationName}");
    Log.CloseAndFlush();
}
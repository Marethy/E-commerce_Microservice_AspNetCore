using Serilog;
using Common.Logging;
using Basket.API.Extensions;
using Basket.API.Persistence;

var builder = WebApplication.CreateBuilder(args);

Log.Information($"Starting {builder.Environment.ApplicationName} ");
try
{
    builder.Host.UseSerilog(SeriLogger.Configure);
    builder.AddAppConfigurations();

    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();
    app.UseInfrastructure();

    //// Migrate database and seed data
    //app.MigrateDatabase<ProductContext>((context, services) =>
    //{
    //    var logger = services.GetService<ILogger<ProductContextSeed>>();
    //    ProductContextSeed.SeedProductAsync(context, logger).Wait();
    //}).Run();
    app.Run();
}
catch (Exception ex)
{
    string type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }
    Log.Fatal(ex, $"Unhandel exception: {ex.Message}");
}
finally
{
    Log.Information("Stopping Basket.API");
    Log.CloseAndFlush();
}



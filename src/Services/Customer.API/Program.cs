
using Common.Logging;
using Customer.API.Controllers;
using Customer.API.Extensions;
using Customer.API.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Information("Starting Customer API up");

try
{
    builder.Host.UseSerilog(Serilogger.Configure);

    builder.Host.AddAppConfigurations();

    builder.Services.AddInfrastructure();

    var app = builder.Build();

    app.UseInfrastructure(builder.Configuration);

    app.MapCustomersAPI();

    app.SeedCustomerData().Run();
}
catch (Exception ex)
{
    string type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

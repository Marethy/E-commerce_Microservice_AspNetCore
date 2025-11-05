using Basket.API.Extensions;
using Common.Logging;
using Serilog;

// Enable HTTP/2 without TLS for gRPC (Docker internal communication)
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

Log.Information("Starting Basket API up");

try
{
    builder.Host.UseSerilog(SeriLogger.Configure);
    builder.Host.AddAppConfigurations();

    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    app.UseInfrastructure();

    app.Run();
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
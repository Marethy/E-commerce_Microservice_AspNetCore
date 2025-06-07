
using Common.Logging;
using HealthChecks.UI.Client;
using Inventory.Product.API.Extensions;
using Inventory.Product.API.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(SeriLogger.Configure);

Log.Information($"Starting {builder.Environment.ApplicationName} up");

try
{
    // Add services to the container.
    builder.Services.AddConfigurationSettings(builder.Configuration);
    builder.Services.AddScoped<InventoryDbSeed>();
    builder.Services.AddRouting(options =>
    {
        options.LowercaseUrls = true;
    });

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
    }); 
    builder.Services.ConfigureMongoDbClient();
    builder.Services.AddInfrastructureServices();
    builder.Services.ConfigureHealthChecks();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    //app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapHealthChecks("/hc", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        endpoints.MapDefaultControllerRoute();
    });

    app.MigrateDatabase().Run();
}
catch (Exception ex)
{
    string type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal)) throw;
    Log.Fatal(ex, $"Unhandled exception: {ex.Message}");
}
finally
{
    Log.Information($"Shut down {builder.Environment.ApplicationName} complete");

    Log.CloseAndFlush();
}

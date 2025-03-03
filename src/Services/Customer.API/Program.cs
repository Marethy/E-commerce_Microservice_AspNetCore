using Serilog;
using Common.Logging;
using Microsoft.EntityFrameworkCore;
using Customer.API.Persistence;
using Customer.API.Repositories.Interfaces;
using Customer.API.Services;
using Contracts.Common.Interfaces;
using Infrastructure.Common;
using Customer.API.Services.Interfaces;
using Customer.API.Controllers;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(SeriLogger.Configure);
Log.Information($"Starting {builder.Environment.ApplicationName} ");
try
{
    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<CustomerContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });

    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>()
        //.AddScoped<IUnitOfWork<CustomerContext>, UnitOfWork<CustomerContext>>()
        .AddScoped(typeof(IRepositoryBase<,,>), typeof(RepositoryBase<,,>))
        .AddScoped(typeof(IRepositoryQueryBase<,,>), typeof(RepositoryQueryBase<,,>))
        .AddScoped<ICustomerService, CustomerService>();

    var app = builder.Build();

    app.MapCustomerController();
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger Customer Minimal API V1");
        });
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    // Gọi SeedCustomerDataAsync trước khi chạy ứng dụng
    await app.SeedCustomerDataAsync();

    // Chạy ứng dụng
    await app.RunAsync();
}
catch (Exception ex)
{
    string type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }
    Log.Fatal(ex, $"Unhandled exception: {ex.Message}");
}
finally
{
    Log.Information($"Starting {builder.Environment.ApplicationName} ");
    Log.CloseAndFlush();
}

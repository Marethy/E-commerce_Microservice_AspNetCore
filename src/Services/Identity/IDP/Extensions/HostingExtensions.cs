using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using IDP.Infrastructure.Domains;
using IDP.Infrastructure.Domains.Interfaces;
using IDP.Infrastructure.Repositories;
using IDP.Infrastructure.Repositories.Interfaces;
using IDP.Middlewares;
using IDP.Services.EmailService;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IDP.Extensions;

internal static class HostingExtensions
{
    public static void AddAppConfigurations(this WebApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true,
                reloadOnChange: true)
            .AddEnvironmentVariables(); 
    }

    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddConfigurationSettings(builder.Configuration);

        builder.Services.AddRazorPages();

        builder.Services.ConfigureIdentity(builder.Configuration);
        builder.Services.ConfigureIdentityServer(builder.Configuration);
        builder.Services.ConfigureCookiePolicy();
        builder.Services.ConfigureCors();
        builder.Services.ConfigureSwagger(builder.Configuration);
        builder.Services.ConfigureAuthentication(builder.Configuration);
        builder.Services.ConfigureAuthorization();
        builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        builder.Services.AddControllers(config =>
        {
            config.RespectBrowserAcceptHeader = true;
            config.ReturnHttpNotAcceptable = true;
            config.Filters.Add(new ProducesAttribute("application/json", "text/plain", "text/json"));
        });

        builder.Services.AddAutoMapper(typeof(Program));

        builder.Services.AddScoped<IEmailSender, SmtpMailService>();
        builder.Services.AddTransient(typeof(IUnitOfWork), typeof(UnitOfWork));
        builder.Services.AddTransient(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));
        builder.Services.AddScoped(typeof(IPermissionRepository), typeof(PermissionRepository));
        builder.Services.AddScoped(typeof(IRepositoryManager), typeof(RepositoryManager));

        builder.Services.ConfigureHealthChecks(builder.Configuration);

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();

        app.UseCors("CorsPolicy");

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API");
            c.OAuthClientId("microservices_swagger");
            c.DisplayRequestDuration();
        });

        app.UseRouting();
        app.UseMiddleware<ErrorWrappingMiddleware>();
        app.UseCookiePolicy();
        app.UseIdentityServer();

        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapRazorPages().RequireAuthorization();

            endpoints.MapHealthChecks("/hc", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        });

        return app;
    }

    private static void ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IdentitySqlConnection");
        services.AddHealthChecks()
             .AddSqlServer(connectionString);
    }

    public static void ConfigureSerilog(this ConfigureHostBuilder host)
    {
        host.UseSerilog((context, configuration) =>
        {
            var elasticUri = context.Configuration.GetValue<string>("ElasticConfiguration:Uri");
            var username = context.Configuration.GetValue<string>("ElasticConfiguration:Username");
            var password = context.Configuration.GetValue<string>("ElasticConfiguration:Password");
            var applicationName = context.HostingEnvironment.ApplicationName?.ToLower().Replace(".", "-");

            if (string.IsNullOrEmpty(elasticUri))
                throw new Exception("ElasticConfiguration Uri is not configured.");

            configuration
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .WriteTo.Debug()
                .WriteTo.Console().ReadFrom.Configuration(context.Configuration)
                .WriteTo.Elasticsearch(
                    new ElasticsearchSinkOptions(new Uri(elasticUri))
                    {
                        IndexFormat =
                            $"{applicationName}-logs-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                        AutoRegisterTemplate = true,
                        NumberOfShards = 2,
                        NumberOfReplicas = 1,
                        ModifyConnectionSettings = x => x.BasicAuthentication(username, password)
                    })
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithProperty("Application", applicationName)
                .ReadFrom.Configuration(context.Configuration);
        });
    }
}

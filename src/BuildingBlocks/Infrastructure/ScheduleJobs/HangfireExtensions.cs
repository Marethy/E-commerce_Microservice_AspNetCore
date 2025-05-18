using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Extensions;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Hangfire.PostgreSql;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;
using Shared.Configurations;
using System.Security.Authentication;

namespace Infrastructure.ScheduleJobs;

public static class HangfireExtensions
{
    public static void AddHangfireService(this IServiceCollection services)
    {
        var settings = services.GetOptions<HangFireSettings>(nameof(HangFireSettings));
        if (settings == null || settings.Storage == null ||
            string.IsNullOrEmpty(settings.Storage.ConnectionString))
        {
            throw new Exception("HangFireSettings is not configured properly!");
        }

        services.ConfigureHangfireServices(settings);
        services.AddHangfireServer(serverOptions => { serverOptions.ServerName = settings.ServerName; });
    }

    public static void UseHangfireDashboard(this IApplicationBuilder app, IConfiguration configuration)
    {
        var configDashboard = configuration.GetSection("HangFireSettings:Dashboard").Get<DashboardOptions>();
        var hangfireSettings = configuration.GetSection(nameof(HangFireSettings)).Get<HangFireSettings>();
        var hangfireRoute = hangfireSettings.Route;

        app.UseHangfireDashboard(hangfireRoute, new DashboardOptions
        {
            Authorization = new[] { new AuthorizationFilter() },
            DashboardTitle = configDashboard.DashboardTitle,
            StatsPollingInterval = configDashboard.StatsPollingInterval,
            AppPath = configDashboard.AppPath,
            IgnoreAntiforgeryToken = true
        });
    }

    private static void ConfigureHangfireServices(this IServiceCollection services,
        HangFireSettings settings)
    {
        if (string.IsNullOrEmpty(settings.Storage.DBProvider))
            throw new Exception("HangFire DBProvider is not configured.");

        switch (settings.Storage.DBProvider.ToLower())
        {
            case "mongodb":
                var mongoUrlBuilder = new MongoUrlBuilder(settings.Storage.ConnectionString);

                var mongoClientSettings = MongoClientSettings.FromUrl(
                    new MongoUrl(settings.Storage.ConnectionString));
                mongoClientSettings.SslSettings = new SslSettings
                {
                    EnabledSslProtocols = SslProtocols.Tls12
                };
                var mongoClient = new MongoClient(mongoClientSettings);

                var mongoStorageOptions = new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy()
                    },
                    CheckConnection = true,
                    Prefix = "SchedulerQueue",
                    CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
                };
                services.AddHangfire((provider, config) =>
                {
                    config.UseSimpleAssemblyNameTypeSerializer()
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                        .UseRecommendedSerializerSettings()
                        .UseConsole()
                        .UseMongoStorage(mongoClient, mongoUrlBuilder.DatabaseName, mongoStorageOptions);

                    var jsonSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    config.UseSerializerSettings(jsonSettings);
                });
                services.AddHangfireConsoleExtensions();
                break;

            case "postgresql":
                services.AddHangfire(x =>
                    x.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(settings.Storage.ConnectionString)));
                break;

            case "mssql":
                services.AddHangfire(configuration => configuration
                   .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                   .UseSimpleAssemblyNameTypeSerializer()
                   .UseRecommendedSerializerSettings()
                   .UseSqlServerStorage(settings.Storage.ConnectionString));
                break;

            default:
                throw new Exception($"HangFire Storage Provider {settings.Storage.DBProvider} is not supported.");
        }
    }
}
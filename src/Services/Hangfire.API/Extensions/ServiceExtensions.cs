using Contracts.ScheduledJobs;
using Contracts.Services;
using Hangfire.API.Services;
using Hangfire.API.Services.Interfaces;
using Infrastructure.Configurations;
using Infrastructure.Extensions;
using Infrastructure.ScheduleJobs;
using Infrastructure.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.Configurations;

namespace Hangfire.API.Extensions;

public static class ServiceExtensions
{
    internal static void AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var hangFireSettings = configuration.GetSection(nameof(HangFireSettings)).Get<HangFireSettings>();
        services.AddSingleton(hangFireSettings);

        var emailSettings = configuration.GetSection(nameof(SMTPEmailSetting)).Get<SMTPEmailSetting>();
        services.AddSingleton(emailSettings);
    }

    internal static void ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IScheduledJobService, HangfireService>()
                .AddScoped<IBackgroundJobService, BackgroundJobService>()
                .AddScoped<ISMTPEmailService, SMTPEmailService>();
    }

    //internal static void ConfigureHealthChecks(this IServiceCollection services)
    //{
    //    var databaseSettings = services.GetOptions<HangFireSettings>(nameof(HangFireSettings));
    //    services.AddHealthChecks().AddMongoDb(databaseSettings.Storage.ConnectionString, "MongoDb Health", HealthStatus.Degraded);
    //}
}
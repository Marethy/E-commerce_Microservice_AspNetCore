using Contracts.ScheduledJobs;
using Contracts.Services;
using Hangfire.API.Services;
using Hangfire.API.Services.Interfaces;
using Infrastructure.Configurations;
using Infrastructure.Extensions;
using Infrastructure.ScheduleJobs;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using Shared.Configurations;

namespace Hangfire.API.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Đăng ký các Setting từ appsettings.json vào DI:
        /// - HangFireSettings (chứa Storage: ConnectionString, DatabaseName)
        /// - SMTPEmailSetting (nếu có)
        /// </summary>
        public static IServiceCollection AddConfigurationSettings(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 1. Lấy HangFireSettings từ Configuration và add Singleton<HangFireSettings>
            var hangFireSettings = configuration
                .GetSection(nameof(HangFireSettings))
                .Get<HangFireSettings>();

            if (hangFireSettings == null)
                throw new ArgumentNullException(
                    nameof(HangFireSettings),
                    "Section 'HangFireSettings' chưa được cấu hình trong appsettings.json");

            services.AddSingleton(hangFireSettings);

            // 2. Lấy SMTPEmailSetting từ Configuration và add Singleton<SMTPEmailSetting>
            var emailSettings = configuration
                .GetSection(nameof(SMTPEmailSetting))
                .Get<SMTPEmailSetting>();

            if (emailSettings == null)
                throw new ArgumentNullException(
                    nameof(SMTPEmailSetting),
                    "Section 'SMTPEmailSetting' chưa được cấu hình trong appsettings.json");

            services.AddSingleton(emailSettings);

            return services;
        }

        /// <summary>
        /// Đăng ký IMongoClient vào DI:
        /// - Lấy ConnectionString và DatabaseName từ HangFireSettings
        /// - Tạo new MongoClient(connectionString) rồi đăng ký Singleton<IMongoClient>
        /// - Đồng thời đăng ký IClientSessionHandle nếu cần dùng session
        /// </summary>
        public static IServiceCollection ConfigureMongoDbClient(
            this IServiceCollection services)
        {
            // Chúng ta dùng BuildServiceProvider tạm để lấy HangFireSettings đã AddSingleton ở trên
            using var sp = services.BuildServiceProvider();
            var settings = sp.GetRequiredService<HangFireSettings>();

            if (settings.Storage == null ||
                string.IsNullOrWhiteSpace(settings.Storage.ConnectionString))
            {
                throw new ArgumentException(
                    "HangFireSettings.Storage.ConnectionString hoặc DatabaseName chưa được cấu hình");
            }

            // 1. Đăng ký IMongoClient
            services.AddSingleton<IMongoClient>(serviceProvider =>
            {
                // Nếu cần authSource hoặc thêm database name vào chuỗi, bạn có thể làm tại đây:
                // var rawConn = settings.Storage.ConnectionString; 
                // var fullConn = $"{rawConn}/{settings.Storage.DatabaseName}?authSource=admin";
                // return new MongoClient(fullConn);

                return new MongoClient(settings.Storage.ConnectionString);
            });

            // 2. Nếu bạn cần session, đăng ký IClientSessionHandle scoped
            services.AddScoped(serviceProvider =>
            {
                var client = serviceProvider.GetRequiredService<IMongoClient>();
                return client.StartSession();
            });

            return services;
        }

        /// <summary>
        /// Đăng ký các lớp Service riêng của Hangfire.API:
        /// - IScheduledJobService => HangfireService
        /// - IBackgroundJobService => BackgroundJobService
        /// - ISMTPEmailService => SMTPEmailService
        /// </summary>
        public static IServiceCollection ConfigureServices(
            this IServiceCollection services)
        {
            services
                .AddScoped<IScheduledJobService, HangfireService>()
                .AddScoped<IBackgroundJobService, BackgroundJobService>()
                .AddScoped<ISMTPEmailService, SMTPEmailService>()
                .AddScoped<IJobMonitorService, JobMonitorService>();

            return services;
        }

        /// <summary>
        /// Đăng ký HealthChecks cho MongoDB:
        /// - Dùng overload AddMongoDb(clientFactory, databaseName, name, failureStatus)
        ///   vì bạn đã register IMongoClient phía trên.
        /// </summary>
        public static IServiceCollection ConfigureHealthChecks(
            this IServiceCollection services)
        {
            // Lấy HangFireSettings từ DI để lấy DatabaseName
            using var sp = services.BuildServiceProvider();
            var settings = sp.GetRequiredService<HangFireSettings>();

            // Đảm bảo IMongoClient đã được đăng ký ở ConfigureMongoDbClient
            services.AddHealthChecks()
                    .AddMongoDb(
                        clientFactory: sp2 => sp2.GetRequiredService<IMongoClient>(),
                        name: "MongoDb Health",
                        failureStatus: HealthStatus.Degraded);

            return services;
        }
    }
}

using Contracts.Common.Interfaces;
using Contracts.ScheduledJobs;
using Infrastructure.Common;
using Infrastructure.Configurations;
using Infrastructure.Extensions;
using Infrastructure.ScheduleJobs;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ordering.API.EventBusConsumer;
using Ordering.API.Services;
using Shared.Configurations;

namespace Ordering.API.Extensions
{
    public static class ServiceExtensions
    {
        internal static IServiceCollection AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var emailSettings = configuration.GetSection(nameof(SMTPEmailSetting))
                .Get<SMTPEmailSetting>();

            services.AddSingleton(emailSettings);

            var eventBusSettings = configuration.GetSection(nameof(EventBusSettings))
                .Get<EventBusSettings>();

            services.AddSingleton(eventBusSettings);

            var urlSettings = configuration.GetSection(nameof(UrlSettings))
                .Get<UrlSettings>();

            services.AddSingleton(urlSettings);

            return services;
        }

        public static void ConfigureMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            if (AppDomain.CurrentDomain.FriendlyName.Contains("ef") || Environment.CommandLine.ToLower().Contains("ef"))
            {
                Console.WriteLine("Skipping MassTransit configuration because EF migration is running.");
                return;  
            }

            var settings = configuration.GetSection(nameof(EventBusSettings)).Get<EventBusSettings>();

            if (settings == null || string.IsNullOrEmpty(settings.HostAddress))
            {
                throw new ArgumentException("EventBusSettings is not configured.");
            }

            services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);

            services.AddMassTransit(x =>
            {
                x.AddConsumersFromNamespaceContaining<BasketCheckoutConsumer>();

                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var eventBusSettings = configuration.GetSection(nameof(EventBusSettings)).Get<EventBusSettings>();
                    cfg.Host(eventBusSettings.HostAddress);

                    cfg.ConfigureEndpoints(context);
                });
            });
        }

        public static void ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddHealthChecks()
                .AddSqlServer(connectionString);
        }

        public static void ConfigureOrderingServices(this IServiceCollection services)
        {
            // Register PDF Service
            services.AddScoped<IOrderPdfService, OrderPdfService>();
        }
    }
}
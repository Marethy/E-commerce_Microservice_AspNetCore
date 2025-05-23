﻿using Infrastructure.Configurations;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ordering.API.EventBusConsumer;
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

            return services;
        }

        public static void ConfigureMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            if (AppDomain.CurrentDomain.FriendlyName.Contains("ef") || Environment.CommandLine.ToLower().Contains("ef"))
            {
                Console.WriteLine("Skipping MassTransit configuration because EF migration is running.");
                return;  // Bỏ qua MassTransit khi chạy EF migration
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
    }
}
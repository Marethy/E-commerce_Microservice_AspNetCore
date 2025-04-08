﻿using Basket.API.Repositories;
using Basket.API.Repositories.Interfaces;
using Contracts.Common.Interfaces;
using Infrastructure.Common;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Configurations;
using MassTransit;
using EventBus.Messages;

namespace Basket.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddAuthorization();
            services.AddLogging();

            services.AddAutoMapper(cfg => cfg.AddProfile(new MappingProfile()));
            services.AddScoped<IBasketRepository, BasketRepository>();
            services.AddTransient<ISerializeService, SerializeService>();
            services.AddSingleton<ILogger<BasketRepository>, Logger<BasketRepository>>();

            // Thêm các cấu hình hệ thống
            services.AddConfigurationSettings(configuration);
            services.ConfigureRedis(configuration);
            services.ConfigureMassTransit(configuration);

            return services;
        }

        private static void AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var eventBusSettings = configuration.GetSection(nameof(EventBusSettings)).Get<EventBusSettings>();
            services.AddSingleton(eventBusSettings);

            var cacheSettings = configuration.GetSection(nameof(CacheSettings)).Get<CacheSettings>();
            services.AddSingleton(cacheSettings);

          // var urlSettings = configuration.GetSection(nameof(UrlSettings)).Get<UrlSettings>();
           // services.AddSingleton(urlSettings);
        }

        private static void ConfigureRedis(this IServiceCollection services, IConfiguration configuration)
        {
            var cacheSettings = configuration.GetSection(nameof(CacheSettings)).Get<CacheSettings>();
            if (cacheSettings == null || string.IsNullOrEmpty(cacheSettings.ConnectionString))
            {
                throw new ArgumentException("Redis ConnectionString is not configured!");
            }

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheSettings.ConnectionString;
            });
        }

        private static void ConfigureMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            var eventBusSettings = configuration.GetSection(nameof(EventBusSettings)).Get<EventBusSettings>();
            if (eventBusSettings == null || string.IsNullOrEmpty(eventBusSettings.HostAddress))
            {
                throw new ArgumentException("EventBusSettings is not configured!");
            }

            var mqConnection = new Uri(eventBusSettings.HostAddress);

            services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
            services.AddMassTransit(config =>
            {
                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(mqConnection);
                });

                config.AddRequestClient<IBasketCheckoutEvent>();
            });
        }
    }
}

using Basket.API.Repositories;
using Basket.API.Repositories.Interfaces;
using Contracts.Common.Interfaces;
using Infrastructure.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            services.AddScoped<IBasketRepository, BasketRepository>();
            services.AddTransient<ISerializeService, SerializeService>();
            services.AddSingleton<ILogger<BasketRepository>, Logger<BasketRepository>>();

            services.ConfigureRedis(configuration);

            return services;
        }

        public static void ConfigureRedis(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration.GetSection("CacheSettings:ConnectionString").Value;
            var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("RedisLogger");

            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                try
                {
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = redisConnectionString;
                    });
                    logger.LogInformation($"Redis connected successfully: {redisConnectionString}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Redis connection error: {ex.Message}");
                }
            }
            else
            {
                logger.LogWarning("Redis ConnectionString is empty or not configured.");
            }
        }
    }
}

using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Basket.API.Repositories.Interfaces;
using Basket.API.Services.Interfaces;
using Basket.API.Services;
using Contracts.Common.Interfaces;
using Contracts.Inventory;
using Contracts.ScheduledJobs;
using Contracts.Services;
using Infrastructure.Common;
using Infrastructure.Extensions;
using Infrastructure.ScheduleJobs;
using Infrastructure.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Configurations;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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
            services.AddScoped<IBasketRepository, BasketRepository>()
           .AddTransient<ISerializeService, SerializeService>()
 .AddScoped<IEmailTemplateService, EmailTemplateService>()
       .AddScoped<IBasketEmailService, BasketEmailService>()
            .AddScoped<IHttpClientHelper, HttpClientHelper>()
       .AddScoped<IScheduledJobsClient, ScheduledJobClient>()
       .AddScoped<IUserActivityService, UserActivityService>();
   services.AddSingleton<ILogger<BasketRepository>, Logger<BasketRepository>>();

            services.AddConfigurationSettings(configuration);
  services.ConfigureRedis(configuration);
          services.ConfigureMassTransit(configuration);
            services.ConfigureGrpcService();
      services.ConfigureHealthChecks();
        
   return services;
        }

private static void AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
        {
var eventBusSettings = configuration.GetSection(nameof(EventBusSettings)).Get<EventBusSettings>();
            services.AddSingleton(eventBusSettings);

            var cacheSettings = configuration.GetSection(nameof(CacheSettings)).Get<CacheSettings>();
         services.AddSingleton(cacheSettings);

          var grpcSettings = configuration.GetSection(nameof(GrpcSettings)).Get<GrpcSettings>();
 services.AddSingleton(grpcSettings);

            var urlSettings = configuration.GetSection(nameof(UrlSettings)).Get<UrlSettings>();
  services.AddSingleton(urlSettings);
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
          config.SetKebabCaseEndpointNameFormatter();

     config.UsingRabbitMq((ctx, cfg) =>
              {
   cfg.Host(new Uri(eventBusSettings.HostAddress));
             cfg.ConfigureEndpoints(ctx);
       });

          config.AddPublishMessageScheduler();
   });
      }

        private static void ConfigureGrpcService(this IServiceCollection services)
        {
            var settings = services.GetOptions<GrpcSettings>(nameof(GrpcSettings));
            
        services.AddGrpcClient<StockProtoService.StockProtoServiceClient>(options =>
      {
  options.Address = new Uri(settings.StockUrl);
        })
  .ConfigurePrimaryHttpMessageHandler(() =>
            {
      var handler = new HttpClientHandler();
  // Allow HTTP/2 without TLS for Docker internal communication
       return handler;
     });
          
services.AddScoped<StockItemGrpcService>();
     }

        private static void ConfigureHealthChecks(this IServiceCollection services)
      {
    var settings = services.GetOptions<CacheSettings>(nameof(CacheSettings));
         services.AddHealthChecks().AddRedis(settings.ConnectionString,
            name: "Redis Health",
     failureStatus: HealthStatus.Degraded);
        }
    }
}
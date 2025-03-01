using Basket.API.Repositories;
using Basket.API.Repositories.Interfaces;
using Contracts.Common.Interfaces;
using Infrastructure.Common;

namespace Basket.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddScoped<IBasketRepository, BasketRepository>()
                .AddTransient<ISerializeService, SerializeService>();
            services.ConfigureRedis(configuration);
            //services.ConfigureProductDbContext(configuration);
            //services.AddInfrastructureServices();
            //services.AddAutoMapper(cfg => cfg.AddProfile(new MappingProfile()));

            return services;
        }
        public static void ConfigureRedis(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration.GetSection("CacheSettings:ConnectionString").Value;
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
            });
        }



    }
}

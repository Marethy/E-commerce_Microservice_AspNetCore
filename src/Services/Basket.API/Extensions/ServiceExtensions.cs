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
            //services.ConfigureProductDbContext(configuration);
            //services.AddInfrastructureServices();
            //services.AddAutoMapper(cfg => cfg.AddProfile(new MappingProfile()));

            return services;
        }

    }
}

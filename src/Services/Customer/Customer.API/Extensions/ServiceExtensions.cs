using Contracts.Common.Interfaces;
using Customer.API.Persistence;
using Customer.API.Repositories;
using Customer.API.Repositories.Interfaces;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using Shared.Configurations;

namespace Customer.API.Extensions
{
    public static class ServiceExtensions
    {
        internal static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddControllers();
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Customer API",
                    Version = "v1",
                    Description = "Customer Management API"
                });
            });

            services.AddConfigurationSettings();
            services.ConfigureCustomerContext();
            services.AddInfrastructureServices();
            services.ConfigureHealthChecks();
            services.AddAutoMapper(cfg => cfg.AddProfile(new CustomerMappingProfile()));

            return services;
        }

        private static IServiceCollection AddConfigurationSettings(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var databaseSettings = configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
            services.AddSingleton(databaseSettings);

            return services;
        }

        private static IServiceCollection ConfigureCustomerContext(this IServiceCollection services)
        {
            var databaseSettings = services.BuildServiceProvider().GetRequiredService<DatabaseSettings>();

            services.AddDbContext<CustomerContext>(options =>
                options.UseNpgsql(databaseSettings.ConnectionString));

            return services;
        }

        private static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepositoryQueryBase<,,>), typeof(RepositoryQueryBase<,,>))
                .AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>))
                .AddScoped<ICustomerRepository, CustomerRepository>();

            return services;
        }

        private static IServiceCollection ConfigureHealthChecks(this IServiceCollection services)
        {
            var databaseSettings = services.BuildServiceProvider().GetRequiredService<DatabaseSettings>();

            services.AddHealthChecks()
                .AddNpgSql(databaseSettings.ConnectionString,
                    name: "CustomerDB Health",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "ready", "db" });

            return services;
        }
    }
}
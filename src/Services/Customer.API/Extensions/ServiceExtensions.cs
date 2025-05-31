using Contracts.Common.Interfaces;
using Customer.API.Persistence;
using Customer.API.Repositories;
using Customer.API.Repositories.Interfaces;
using Customer.API.Services;
using Customer.API.Services.Interfaces;
using Infrastructure.Common;
using Infrastructure.Extensions;
using Infrastructure.ScheduleJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.Configurations;

namespace Customer.API.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.ConfigureCustomerDbContext();
            services.AddInfrastructureService();
            services.AddHangfireService();
            services.ConfigureHealthChecks();
        }

        private static void ConfigureCustomerDbContext(this IServiceCollection services)
        {
            var databaseSettings = services.GetOptions<DatabaseSettings>(nameof(DatabaseSettings));
            if (databaseSettings == null || string.IsNullOrEmpty(databaseSettings.ConnectionString))
                throw new ArgumentNullException("Connection string is not configured.");

            services.AddDbContext<CustomerContext>(options => options.UseNpgsql(databaseSettings.ConnectionString));
        }

        private static void AddInfrastructureService(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepositoryBaseAsync<,,>), typeof(RepositoryBaseAsync<,,>))
                    .AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>))
                    .AddScoped<ICustomerRepository, CustomerRepository>()
                    .AddScoped<ICustomerService, CustomerService>();
        }

        private static void ConfigureHealthChecks(this IServiceCollection services)
        {
            var databaseSettings = services.GetOptions<DatabaseSettings>(nameof(DatabaseSettings));
            services.AddHealthChecks().AddNpgSql(databaseSettings.ConnectionString,
                                                 name: "PostgresQL Health",
                                                 failureStatus: HealthStatus.Degraded);
        }
    }
}
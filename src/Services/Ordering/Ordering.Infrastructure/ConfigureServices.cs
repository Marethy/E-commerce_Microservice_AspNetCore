using Contracts.Common.Interfaces;
using Contracts.Messages;
using Contracts.ScheduledJobs;
using Contracts.Services;
using Infrastructure.Common;
using Infrastructure.Configurations;
using Infrastructure.Messages;
using Infrastructure.ScheduleJobs;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Common.Interfaces;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Repositories;
using Shared.Configurations;
using Shared.Services.Email;
using System.Reflection;

namespace Ordering.Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OrderContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(OrderContext).GetTypeInfo().Assembly.GetName().Name);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    }));

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUnitOfWork<OrderContext>, UnitOfWork<OrderContext>>();
            services.AddScoped<IMessageProducer, RabbitMQProducer>();
            services.AddScoped<ISerializeService, SerializeService>();
            services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
            services.AddScoped<IHttpClientHelper, HttpClientHelper>();
            services.AddScoped<IScheduledJobsClient, ScheduledJobClient>();
            services.AddHttpClient();


            services.Configure<SMTPEmailSetting>(configuration.GetSection("SMTPSettings"));

            var urlSettings = configuration.GetSection(nameof(UrlSettings)).Get<UrlSettings>();
            if (urlSettings != null)
            {
                services.AddSingleton(urlSettings);
            }

            services.AddSingleton<ISMTPEmailService<MailRequestDto>, SMTPEmailService>();
            services.AddSingleton<ISMTPEmailService, SMTPEmailService>();

            return services;
        }
    }
}
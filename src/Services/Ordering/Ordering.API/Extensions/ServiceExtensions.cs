using Infrastructure.Configurations;

namespace Ordering.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddEmailSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var emailSettings = new SMTPEmailSetting();
            configuration.GetSection(nameof(SMTPEmailSetting)).Bind(emailSettings);
            services.AddSingleton(emailSettings);
            return services;
        }
    }
}

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Configurations;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Infrastructure.Extensions;

namespace Infrastructure.Identity
{
    public static class ConfigureAuthAuthorHandler
    {
        public static void ConfigureAuthenticationHandler(this IServiceCollection services)
        {
            var configuration = services.GetOptions<ApiConfiguration>("ApiConfiguration");
            if (configuration == null ||
                string.IsNullOrEmpty(configuration.IssuerUri) ||
                string.IsNullOrEmpty(configuration.ApiName))
            {
                throw new Exception("ApiConfiguration is not configured!");
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.Authority = configuration.IssuerUri;
                        options.Audience = configuration.ApiName;
                        options.RequireHttpsMetadata = false;

                        // Bật logging chi tiết để xem quá trình fetch discovery & JWKS
                        options.Events = new JwtBearerEvents
                        {
                            OnAuthenticationFailed = ctx =>
                            {
                                Console.WriteLine($"[Jwt] Auth failed: {ctx.Exception.Message}");
                                return Task.CompletedTask;
                            },
                            OnMessageReceived = ctx =>
                            {
                                Console.WriteLine($"[Jwt] Token received: {ctx.Request.Headers["Authorization"]}");
                                return Task.CompletedTask;
                            },
                            OnTokenValidated = ctx =>
                            {
                                Console.WriteLine("[Jwt] Token validated successfully");
                                return Task.CompletedTask;
                            }
                        };

                        // (Tùy chọn) Tăng control over validation
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true
                        };
                    });
        }

        public static void ConfigureAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                });
            });
        }
    }
}

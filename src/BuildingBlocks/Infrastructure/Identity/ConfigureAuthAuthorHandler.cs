using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Configurations;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Infrastructure.Extensions;
using System.Text;

namespace Infrastructure.Identity
{
    public static class ConfigureAuthAuthorHandler
    {
        public static void ConfigureAuthenticationHandler(this IServiceCollection services)
        {
            var configuration = services.GetOptions<ApiConfiguration>("ApiConfiguration");
            var jwtSettings = services.GetOptions<JwtSettings>(nameof(JwtSettings));

            if (configuration == null ||
                string.IsNullOrEmpty(configuration.IssuerUri) ||
                string.IsNullOrEmpty(configuration.ApiName))
            {
                throw new Exception("ApiConfiguration is not configured!");
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = false;

                        // Use JWT Key validation if available, otherwise use IdentityServer
                        if (jwtSettings != null && !string.IsNullOrEmpty(jwtSettings.Key))
                        {
                            // JWT Key-based validation
                            var key = Encoding.UTF8.GetBytes(jwtSettings.Key);
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidIssuer = jwtSettings.Issuer ?? configuration.IssuerUri,
                                ValidateAudience = true,
                                ValidAudience = jwtSettings.Audience ?? configuration.ApiName,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey = new SymmetricSecurityKey(key),
                                ClockSkew = TimeSpan.Zero
                            };
                        }
                        else
                        {
                            // IdentityServer OAuth2 validation
                            options.Authority = configuration.IssuerUri;
                            options.Audience = configuration.ApiName;

                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true
                            };
                        }

                        // Logging events
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

using Contracts.Identity;
using Infrastructure.Extensions;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Provider.Polly;
using Shared.Configurations;
using System.Text;

namespace OcelotApiGw.Extensions;

public static class ServiceExtensions
{
    internal static void AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
        services.AddSingleton(jwtSettings);
    }

    public static void ConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration["AllowOrigins"];
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
                builder.WithOrigins(origins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            );
        });
    }

    public static void ConfigureOcelot(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOcelot(configuration)
                .AddPolly()
                .AddCacheManager(x => x.WithDictionaryHandle());
        services.AddTransient<ITokenService, TokenService>();
        services.AddJwtAuthentication();
        services.AddSwaggerForOcelot(configuration, x =>
        {
            x.GenerateDocsForGatewayItSelf = true;
        });
    }

    internal static void AddJwtAuthentication(this IServiceCollection services)
    {
        var settings = services.GetOptions<JwtSettings>(nameof(JwtSettings));
        if (settings == null || string.IsNullOrEmpty(settings.Key))
        {
            throw new ArgumentNullException($"{nameof(JwtSettings)} is not configured properly");
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero,
            RequireExpirationTime = false
        };
        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.SaveToken = true;
            x.RequireHttpsMetadata = false;
            x.TokenValidationParameters = tokenValidationParameters;
        });
    }
}
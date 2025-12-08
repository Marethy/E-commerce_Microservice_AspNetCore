﻿using Contracts.Identity;
using Infrastructure.Extensions;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Provider.Polly;
using Shared.Configurations;
using System.Text;
using Ocelot.Cache.CacheManager;

namespace OcelotApiGw.Extensions;

public static class ServiceExtensions
{
    internal static void AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
        services.AddSingleton(jwtSettings);

        var apiConfiguration = configuration.GetSection(nameof(ApiConfiguration)).Get<ApiConfiguration>();
        services.AddSingleton(apiConfiguration);
    }

    public static void ConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration["AllowOrigins"];
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                if (origins == "*")
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                }
                else
                {
                    builder.WithOrigins(origins.Split(','))
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                }
            });
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
        var apiConfig = services.GetOptions<ApiConfiguration>(nameof(ApiConfiguration));
        if (apiConfig == null || string.IsNullOrEmpty(apiConfig.IssuerUri) || string.IsNullOrEmpty(apiConfig.ApiName))
            throw new ArgumentNullException("ApiConfiguration");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = apiConfig.IssuerUri;
            options.Audience = apiConfig.ApiName;
            options.RequireHttpsMetadata = false;          

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = apiConfig.IssuerUri,
                ValidateAudience = true,
                ValidAudience = apiConfig.ApiName,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true
            };
            options.IncludeErrorDetails = true;
        });
    }
}
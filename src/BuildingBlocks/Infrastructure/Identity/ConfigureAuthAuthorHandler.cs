
using IdentityServer4.AccessTokenValidation;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Configurations;

namespace Infrastructure.Identity;

public static class ConfigureAuthAuthorHandler
{
    public static void ConfigureAuthenticationHandler(this IServiceCollection services)
    {
        var configuration = services.GetOptions<ApiConfiguration>("ApiConfiguration");
        if (configuration == null || string.IsNullOrEmpty(configuration.IssuerUri) || string.IsNullOrEmpty(configuration.ApiName))
            throw new Exception("ApiConfiguration is not configured!");

        var issuerUri = configuration.IssuerUri;
        var apiName = configuration.ApiName;
        var authScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;

        services.AddAuthentication(authScheme)
                .AddIdentityServerAuthentication(authScheme, opt =>
                {
                    opt.Authority = issuerUri;
                    opt.ApiName = apiName;
                    opt.RequireHttpsMetadata = false;
                    opt.SupportedTokens = SupportedTokens.Both;
                });
    }

    public static void ConfigureAuthorization(this IServiceCollection services)
    {
        var authScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
        services.AddAuthorization(options =>
        {
            options.AddPolicy(authScheme, policy =>
            {
                policy.AddAuthenticationSchemes(authScheme);
                policy.RequireAuthenticatedUser();
            });
        });
    }
}
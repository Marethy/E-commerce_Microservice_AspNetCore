using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using IDP.Common;
using IDP.Infrastructure.Entities;
using IDP.Infrastructure.Extensions;
using IDP.Infrastructure.Persistence;
using Contracts.Identity;
using Infrastructure.Identity;
using Shared.Configurations;

namespace IDP.Extensions;

public static class ServiceExtensions
{
    internal static void AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var emailSettings = configuration.GetSection(nameof(SMTPEmailSetting)).Get<SMTPEmailSetting>();
        services.AddSingleton(emailSettings);
        
        var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
        services.AddSingleton(jwtSettings);
    }

    public static void ConfigureIdentityServer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IdentitySqlConnection");
        var migrationsAssembly = typeof(Program).Assembly.GetName().Name;
        services.AddIdentityServer(options =>
        {
            // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
            options.EmitStaticAudienceClaim = true;
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;
        })
            // not recommended for production - you need to store your key material somewhere secure
            .AddDeveloperSigningCredential()
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly));
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly));
            })
            .AddAspNetIdentity<User>()
            .AddProfileService<IdentityProfileService>();
    }

    public static void ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin()
                                                              .AllowAnyMethod()
                                                              .AllowAnyHeader());
        });
    }

    public static void ConfigureIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IdentitySqlConnection");
        services.AddDbContext<TeduIdentityContext>(options => options.UseSqlServer(connectionString))
                .AddIdentity<User, IdentityRole>(opt =>
                {
                    opt.Password.RequireDigit = false;
                    opt.Password.RequiredLength = 6;
                    opt.Password.RequireUppercase = false;
                    opt.Password.RequireLowercase = false;
                    opt.User.RequireUniqueEmail = true;
                    opt.Lockout.AllowedForNewUsers = true;
                    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    opt.Lockout.MaxFailedAccessAttempts = 3;
                })
                .AddEntityFrameworkStores<TeduIdentityContext>()
                .AddUserStore<TeduUserStore>()
                .AddDefaultTokenProviders();
        
      services.AddScoped<ITokenService, TokenService>();
    }

    public static void ConfigureSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Identity Server API",
                Version = "v1",
                Contact = new OpenApiContact
                {
                    Name = "Identity Service",
                    Email = "khiemmm2004@gmail.com",
                    Url = new Uri("https://khiem2004@gmail.com")
                }
            });

            var identityServerBaseUrl = configuration.GetSection("IdentityServer:BaseUrl").Value;
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{identityServerBaseUrl}/connect/authorize"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "microservices_api.read", "Microservices API Read Scope" },
                            { "microservices_api.write", "Microservices API Write Scope" }
                        }
                    }
                }
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    new List<string>
                    {
                        "microservices_api.read",
                        "microservices_api.write"
                    }
                }
            });
        });
    }

    public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
        
        services.AddAuthentication()
                .AddLocalApi("LocalApi", option =>
                {
                    option.ExpectedScope = "microservices_api.read";
                })
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings!.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(jwtSettings.Key))
                    };
                });
    }

    public static void ConfigureAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(option =>
        {
            option.AddPolicy("Bearer", policy =>
            {
                policy.AddAuthenticationSchemes("Bearer");
                
                policy.RequireAuthenticatedUser();
            });
        });
    }
}

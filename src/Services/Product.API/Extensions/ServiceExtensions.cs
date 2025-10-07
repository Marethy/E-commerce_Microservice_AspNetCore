using Contracts.Common.Interfaces;
using Infrastructure.Common;
using Infrastructure.Extensions;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Product.API.Persistence;
using Product.API.Repositories;
using Product.API.Repositories.Interfaces;
using Shared.Configurations;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Npgsql;
using HealthChecks.NpgSql;

namespace Product.API.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.ConfigureSwagger();

            services.ConfigureProductDbContext(configuration);
            services.AddInfrastructrueService();
            services.AddAutoMapper(cfg => cfg.AddProfile(new MappingProfile()));
            //       services.AddJwtAuthentication();
            services.ConfigureAuthenticationHandler();
            //        services.ConfigureAuthorization();
            services.ConfigureHealthChecks();
        }




        private static void ConfigureProductDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var databaseSettings = services.GetOptions<DatabaseSettings>(nameof(DatabaseSettings));
            if (databaseSettings == null || string.IsNullOrEmpty(databaseSettings.ConnectionString))
                throw new ArgumentNullException("Connection string is not configured.");

            services.AddDbContext<ProductContext>(options =>
                options.UseNpgsql(databaseSettings.ConnectionString, e =>
                {
                    e.MigrationsAssembly("Product.API");
                }));
        }
        private static void AddInfrastructrueService(this IServiceCollection services)
        {
            // Register base repository and unit of work
            services.AddScoped(typeof(IRepositoryBase<,,>), typeof(RepositoryBase<,,>))
                    .AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));

            // Register specific repositories
            services.AddScoped<IProductRepository, ProductRepository>()
                    .AddScoped<ICategoryRepository, CategoryRepository>()
                    .AddScoped<IProductReviewRepository, ProductReviewRepository>();
        }

        private static void ConfigureHealthChecks(this IServiceCollection services)
        {
            var databaseSettings = services.GetOptions<DatabaseSettings>(nameof(DatabaseSettings));
            services
                    .AddHealthChecks()
                    .AddNpgSql(
                            connectionString: databaseSettings.ConnectionString,
                            name: "PostgreSQL Health",
                            failureStatus: HealthStatus.Degraded,
                            tags: new[] { "ready", "sql" }
                    );
        }

        private static void ConfigureSwagger(this IServiceCollection services)
        {
            var configuration = services.GetOptions<ApiConfiguration>("ApiConfiguration");
            if (configuration == null || string.IsNullOrEmpty(configuration.IssuerUri) ||
                string.IsNullOrEmpty(configuration.ApiName)) throw new Exception("ApiConfiguration is not configured!");

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Product API V1",
                        Version = configuration.ApiVersion
                    });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{configuration.IdentityServerBaseUrl}/connect/authorize"),
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
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Name = "Bearer"
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
    }
}
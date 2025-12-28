using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using IDP.Infrastructure.Common;
using IDP.Infrastructure.Entities;
using Microsoft.Data.SqlClient;

namespace IDP.Infrastructure.Persistence;

public class SeedUserData
{
    public static async Task EnsureSeedDataAsync(string connectionString)
    {
        Console.WriteLine("=== Starting Identity Data Seeding ===");
        Console.WriteLine($"Connection String: {connectionString?.Substring(0, Math.Min(50, connectionString?.Length ?? 0))}...");
        
        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<TeduIdentityContext>(opt => opt.UseSqlServer(connectionString))
                    .AddIdentity<User, IdentityRole>(opt =>
                    {
                        opt.Password.RequireDigit = false;
                        opt.Password.RequiredLength = 6;
                        opt.Password.RequireUppercase = false;
                        opt.Password.RequireLowercase = false;
                        opt.Password.RequireNonAlphanumeric = false;
                    })
                   .AddEntityFrameworkStores<TeduIdentityContext>()
                   .AddDefaultTokenProviders();

            await using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            Console.WriteLine("Migrating TeduIdentityContext database...");
            await using var context = scope.ServiceProvider.GetRequiredService<TeduIdentityContext>();
            context.Database.SetConnectionString(connectionString);
            await context.Database.MigrateAsync();
            Console.WriteLine("✓ Database migration completed");

            // Create stored procedures
            Console.WriteLine("Creating stored procedures...");
            await CreateStoredProceduresAsync(context);
            Console.WriteLine("✓ Stored procedures created");

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            
            // Seed Roles
            Console.WriteLine("Seeding roles...");
            await SeedRolesAsync(roleManager);
            Console.WriteLine("? Roles seeded");
            
            // Seed Users with Roles
            Console.WriteLine("Seeding users...");
            await CreateUserAsync(scope, "Admin", "User", "123 Admin St, Wollongong", Guid.NewGuid().ToString(), "Admin@123$", "Administrator", "admin@tedu.com.vn");
            await CreateUserAsync(scope, "Customer", "User", "456 Customer Ave, Melbourne", Guid.NewGuid().ToString(), "Customer@123$", "Customer", "customer@tedu.com.vn");
            await CreateUserAsync(scope, "Agent", "User", "789 Agent Blvd, Sydney", Guid.NewGuid().ToString(), "Agent@123$", "Agent", "agent@tedu.com.vn");
            Console.WriteLine("? Users seeded");
            
            // Seed Permissions
            Console.WriteLine("Seeding permissions...");
            await SeedPermissionsAsync(context, roleManager);
            Console.WriteLine("? Permissions seeded");
            
            Console.WriteLine("=== Identity Data Seeding Completed Successfully ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? SEED ERROR: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[]
        {
            "Administrator",  // Full system access
            "Customer",       // Customer portal access
            "Agent"          // Agent operations (sales, support, warehouse combined)
        };

        foreach (var roleName in roles)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task SeedPermissionsAsync(TeduIdentityContext context, RoleManager<IdentityRole> roleManager)
    {
        if (context.Permissions.Any())
        {
            return; // Already seeded
        }

        var permissions = new List<Permission>();
        
        // Get all roles
        var adminRole = await roleManager.FindByNameAsync("Administrator");
        var customerRole = await roleManager.FindByNameAsync("Customer");
        var agentRole = await roleManager.FindByNameAsync("Agent");

        // ADMINISTRATOR - Full Access to Everything
        if (adminRole != null)
        {
            AddFullPermissions(permissions, adminRole.Id, "PRODUCT", "ORDER", "CUSTOMER", "INVENTORY", "BASKET", "PAYMENT", "SHIPPING", "REPORTING");
        }

        // CUSTOMER - Full Access to Everything (for testing)
        if (customerRole != null)
        {
            AddFullPermissions(permissions, customerRole.Id, "PRODUCT", "ORDER", "CUSTOMER", "INVENTORY", "BASKET", "PAYMENT", "SHIPPING", "REPORTING");
        }

        // AGENT - Full Access to Everything (for testing)
        if (agentRole != null)
        {
            AddFullPermissions(permissions, agentRole.Id, "PRODUCT", "ORDER", "CUSTOMER", "INVENTORY", "BASKET", "PAYMENT", "SHIPPING", "REPORTING");
        }

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();
    }

    private static void AddFullPermissions(List<Permission> permissions, string roleId, params string[] functions)
    {
        var commands = new[] { "CREATE", "UPDATE", "DELETE", "VIEW", "IMPORT", "EXPORT" };
        
        foreach (var function in functions)
        {
            foreach (var command in commands)
            {
                permissions.Add(new Permission(function, command, roleId));
            }
        }
    }

    private static async Task CreateUserAsync(IServiceScope scope, string firstName, string lastName, string address,
                        string id, string password, string role, string email)
    {
        var userManagement = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManagement.FindByNameAsync(email);
        if (user == null)
        {
            user = new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Address = address,
                EmailConfirmed = true,
                Id = id,
            };
            var result = await userManagement.CreateAsync(user, password);
            CheckResult(result);

            var addToRoleResult = await userManagement.AddToRoleAsync(user, role);
            CheckResult(addToRoleResult);

            result = userManagement.AddClaimsAsync(user, new Claim[]
            {
                new(SystemConstants.Claims.UserName, user.UserName),
                new(SystemConstants.Claims.FirstName, user.FirstName),
                new(SystemConstants.Claims.LastName, user.LastName),
                new(SystemConstants.Claims.Roles, role),
                new(JwtClaimTypes.Address, user.Address),
                new(JwtClaimTypes.Email, user.Email),
                new(ClaimTypes.NameIdentifier, user.Id),
            }).Result;
            CheckResult(result);
        }
    }

    private static void CheckResult(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new Exception(result.Errors.First().Description);
        }
    }

    private static async Task CreateStoredProceduresAsync(TeduIdentityContext context)
    {
        try
        {
            // 1. Create table type first
            await context.Database.ExecuteSqlRawAsync(@"
                IF EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'Permission')
                    DROP TYPE [dbo].[Permission];
                
                CREATE TYPE [dbo].[Permission] AS TABLE(
                    [Function] VARCHAR(50) NOT NULL,
                    [Command] VARCHAR(50) NOT NULL,
                    [RoleId] VARCHAR(50) NOT NULL
                );
            ");

            // 2. Get_Permission_By_RoleId
            await context.Database.ExecuteSqlRawAsync(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Get_Permission_By_RoleId')
                    DROP PROCEDURE [Get_Permission_By_RoleId];
            ");
            
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE PROCEDURE [Get_Permission_By_RoleId] @roleId varchar(50) null
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT * FROM [Identity].Permissions WHERE RoleId = @roleId
                END
            ");

            // 3. Create_Permission
            await context.Database.ExecuteSqlRawAsync(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Create_Permission')
                    DROP PROCEDURE [Create_Permission];
            ");
            
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE PROCEDURE [Create_Permission] 
                    @roleId VARCHAR(50) NULL,
                    @function VARCHAR(50) NULL,
                    @command VARCHAR(50) NULL,
                    @newId BIGINT OUTPUT
                AS
                BEGIN
                    SET XACT_ABORT ON;
                    BEGIN TRAN
                        BEGIN TRY 
                            IF NOT EXISTS (SELECT * FROM [Identity].Permissions WHERE [RoleId] = @roleId AND [Function] = @function AND [Command] = @command)
                            BEGIN
                                INSERT INTO [Identity].Permissions ([RoleId], [Function], [Command]) VALUES (@roleId, @function, @command)
                                SET @newId = SCOPE_IDENTITY();
                            END
                        COMMIT
                        END TRY
                        BEGIN CATCH 
                            ROLLBACK
                            DECLARE @ErrorMessage VARCHAR(2000)
                            SELECT @ErrorMessage = 'ERROR: ' + ERROR_MESSAGE() 
                            RAISERROR(@ErrorMessage, 16, 1)
                        END CATCH
                END
            ");

            // 4. Delete_Permission
            await context.Database.ExecuteSqlRawAsync(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Delete_Permission')
                    DROP PROCEDURE [Delete_Permission];
            ");
            
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE PROCEDURE [Delete_Permission] 
                    @roleId VARCHAR(50) NULL,
                    @function VARCHAR(50) NULL,
                    @command VARCHAR(50) NULL
                AS
                BEGIN
                    DELETE FROM [Identity].Permissions WHERE [RoleId] = @roleId AND [Function] = @function AND [Command] = @command
                END
            ");

            // 5. Update_Permissions_By_RoleId
            await context.Database.ExecuteSqlRawAsync(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Update_Permissions_By_RoleId')
                    DROP PROCEDURE [Update_Permissions_By_RoleId];
            ");
            
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE PROCEDURE [Update_Permissions_By_RoleId] 
                    @roleId VARCHAR(50) NULL,
                    @permissions Permission READONLY
                AS
                BEGIN
                    SET XACT_ABORT ON;
                    BEGIN TRAN
                        BEGIN TRY
                            DELETE FROM [Identity].Permissions WHERE RoleId = @roleId
                            INSERT INTO [Identity].Permissions SELECT [Function], [Command], [RoleId] FROM @permissions
                        COMMIT
                        END TRY
                        BEGIN CATCH
                            ROLLBACK
                            DECLARE @ErrorMessage VARCHAR(2000)
                            SELECT @ErrorMessage = 'ERROR: ' + ERROR_MESSAGE()
                            RAISERROR(@ErrorMessage, 16, 1)
                        END CATCH
                END
            ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not create stored procedures: {ex.Message}");
            // Don't throw - allow app to continue
        }
    }
}
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
            await CreateUserAsync(scope, "Admin", "User", "123 Admin St, Wollongong", "+84 901 234 567", Guid.NewGuid().ToString(), "Admin@123$", "Administrator", "admin@tedu.com.vn");
            await CreateUserAsync(scope, "Customer", "User", "456 Customer Ave, Melbourne", "+84 912 345 678", Guid.NewGuid().ToString(), "Customer@123$", "Customer", "customer@tedu.com.vn");
            await CreateUserAsync(scope, "Agent", "User", "789 Agent Blvd, Sydney", "+84 923 456 789", Guid.NewGuid().ToString(), "Agent@123$", "Agent", "agent@tedu.com.vn");
            Console.WriteLine("✓ Users seeded");                                                   
            
            // Seed Bulk Users (1100 users)
            Console.WriteLine("Seeding bulk users (1100 users)...");
            await SeedBulkUsersAsync(scope,roleManager);
            Console.WriteLine("✓ Bulk users seeded");
            
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

    private static async Task CreateUserAsync(IServiceScope scope, string firstName, string lastName, string address, string phoneNumber,
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
                PhoneNumber = phoneNumber,
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

    private static async Task SeedBulkUsersAsync(IServiceScope scope, RoleManager<IdentityRole> roleManager)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        
        var existingCount = await userManager.Users.CountAsync();
        Console.WriteLine($"  Current user count: {existingCount}. Checking integrity...");

        var random = new Random(12345); // Fixed seed for consistency
        
        // Vietnamese names
        var firstNames = new[] { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương", "Lý" };
        var middleNames = new[] { "Văn", "Thị", "Minh", "Hữu", "Thanh", "Đức", "Quốc", "Hồng", "Thu", "Phương", "Tấn", "Ngọc", "Anh", "Bảo", "Châu" };
        var lastNames = new[] { "An", "Bình", "Cường", "Dũng", "Hùng", "Khoa", "Long", "Nam", "Phong", "Quân", "Sơn", "Tài", "Trung", "Tuấn", "Vũ", "Hà", "Hoa", "Lan", "Mai", "Nga", "Oanh", "Phương", "Quỳnh", "Trang" };
        
        int created = 0;
        int updated = 0;

        // Helper to update claims
        async Task UpdateClaimsAsync(User user, string role) {
             var currentClaims = await userManager.GetClaimsAsync(user);
             var nameClaim = currentClaims.FirstOrDefault(c => c.Type == SystemConstants.Claims.FirstName);
             var lastNameClaim = currentClaims.FirstOrDefault(c => c.Type == SystemConstants.Claims.LastName);
             
             if (nameClaim != null) await userManager.RemoveClaimAsync(user, nameClaim);
             if (lastNameClaim != null) await userManager.RemoveClaimAsync(user, lastNameClaim);
             
             await userManager.AddClaimAsync(user, new Claim(SystemConstants.Claims.FirstName, user.FirstName));
             await userManager.AddClaimAsync(user, new Claim(SystemConstants.Claims.LastName, user.LastName));
        }

        // 1. Seed 1000 Customer users
        Console.WriteLine("  Processing 1000 Customer users...");
        for (int i = 1; i <= 1000; i++)
        {
            var username = $"user{i}";
            var firstName = firstNames[random.Next(firstNames.Length)];
            var middleName = middleNames[random.Next(middleNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var fullName = $"{middleName} {lastName}";
            
            var existingUser = await userManager.FindByNameAsync(username);
            if (existingUser == null)
            {
                var user = new User
                {
                    UserName = username,
                    Email = $"{username}@tedu.com.vn",
                    FirstName = firstName,
                    LastName = fullName,
                    Address = $"{random.Next(1, 999)} Street, District {random.Next(1, 12)}, Ho Chi Minh",
                    PhoneNumber = $"09{random.Next(10000000, 99999999)}",
                    EmailConfirmed = true,
                    Id = Guid.NewGuid().ToString()
                };

                var result = await userManager.CreateAsync(user, "Password@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Customer");
                    await userManager.AddClaimsAsync(user, new[]
                    {
                        new Claim(SystemConstants.Claims.UserName, user.UserName),
                        new Claim(SystemConstants.Claims.FirstName, user.FirstName),
                        new Claim(SystemConstants.Claims.LastName, user.LastName),
                        new Claim(SystemConstants.Claims.Roles, "Customer"),
                        new Claim(JwtClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.Id)
                    });
                    created++;
                }
            } 
            else 
            {
                if (existingUser.FirstName != firstName || existingUser.LastName != fullName) 
                {
                    existingUser.FirstName = firstName;
                    existingUser.LastName = fullName;
                    await userManager.UpdateAsync(existingUser);
                    await UpdateClaimsAsync(existingUser, "Customer");
                    updated++;
                }
            }
            
            if (i % 200 == 0) Console.WriteLine($"    Processed {i}/1000 Customer users...");
        }

        // 2. Seed 20 Administrator users
        Console.WriteLine("  Processing 20 Administrator users...");
        for (int i = 1; i <= 20; i++)
        {
            var username = $"admin{1000 + i}";
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            
            var existingUser = await userManager.FindByNameAsync(username);
            if (existingUser == null)
            {
                var user = new User
                {
                    UserName = username,
                    Email = $"{username}@tedu.com.vn",
                    FirstName = firstName,
                    LastName = lastName,
                    Address = $"Admin Office {i}, District 1, Ho Chi Minh",
                    PhoneNumber = $"09{random.Next(10000000, 99999999)}",
                    EmailConfirmed = true,
                    Id = Guid.NewGuid().ToString()
                };

                var result = await userManager.CreateAsync(user, "Password@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Administrator");
                    await userManager.AddClaimsAsync(user, new[]
                    {
                        new Claim(SystemConstants.Claims.UserName, user.UserName),
                        new Claim(SystemConstants.Claims.FirstName, user.FirstName),
                        new Claim(SystemConstants.Claims.LastName, user.LastName),
                        new Claim(SystemConstants.Claims.Roles, "Administrator"),
                        new Claim(JwtClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.Id)
                    });
                    created++;
                }
            }
            else 
            {
                if (existingUser.FirstName != firstName || existingUser.LastName != lastName) 
                {
                    existingUser.FirstName = firstName;
                    existingUser.LastName = lastName;
                    await userManager.UpdateAsync(existingUser);
                    await UpdateClaimsAsync(existingUser, "Administrator");
                    updated++;
                }
            }
        }

        // 3. Seed 80 Agent users
        Console.WriteLine("  Processing 80 Agent users...");
        for (int i = 1; i <= 80; i++)
        {
            var username = $"agent{1020 + i}";
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            
            var existingUser = await userManager.FindByNameAsync(username);
            if (existingUser == null)
            {
                var user = new User
                {
                    UserName = username,
                    Email = $"{username}@tedu.com.vn",
                    FirstName = firstName,
                    LastName = lastName,
                    Address = $"Agent Office {i}, District {random.Next(1, 12)}, Ho Chi Minh",
                    PhoneNumber = $"09{random.Next(10000000, 99999999)}",
                    EmailConfirmed = true,
                    Id = Guid.NewGuid().ToString()
                };

                var result = await userManager.CreateAsync(user, "Password@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Agent");
                    await userManager.AddClaimsAsync(user, new[]
                    {
                        new Claim(SystemConstants.Claims.UserName, user.UserName),
                        new Claim(SystemConstants.Claims.FirstName, user.FirstName),
                        new Claim(SystemConstants.Claims.LastName, user.LastName),
                        new Claim(SystemConstants.Claims.Roles, "Agent"),
                        new Claim(JwtClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.Id)
                    });
                    created++;
                }
            }
            else 
            {
                if (existingUser.FirstName != firstName || existingUser.LastName != lastName) 
                {
                    existingUser.FirstName = firstName;
                    existingUser.LastName = lastName;
                    await userManager.UpdateAsync(existingUser);
                    await UpdateClaimsAsync(existingUser, "Agent");
                    updated++;
                }
            }
        }

        Console.WriteLine($"  ✅ Process finished: {created} created, {updated} updated/fixed.");
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
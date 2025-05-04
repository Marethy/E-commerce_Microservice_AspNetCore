using Inventory.Product.API.Persistence;
using MongoDB.Driver;
using Shared.Configurations;
using Microsoft.Extensions.Logging;

namespace Inventory.Product.API.Extensions
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            // Use a non-static type for the logger
            var logger = services.GetRequiredService<ILogger<MigrateDatabaseLogger>>();

            var settings = services.GetService<MongoDbSettings>();

            if (settings == null || string.IsNullOrEmpty(settings.ConnectionString))
            {
                logger.LogError("DatabaseSettings are not configured.");
                throw new ArgumentNullException("DatabaseSettings is not configured");
            }

            try
            {
                var mongoClient = services.GetRequiredService<IMongoClient>();
                logger.LogInformation("Starting database migration and seeding.");

                var inventoryDbSeed = services.GetRequiredService<InventoryDbSeed>();
                inventoryDbSeed.SeedDataAsync(mongoClient, settings).Wait();

                logger.LogInformation("Database migration and seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the database migration and seeding process.");
                throw;
            }

            return host;
        }

        // Define a non-static class for logging purposes
        private class MigrateDatabaseLogger { }
    }
}

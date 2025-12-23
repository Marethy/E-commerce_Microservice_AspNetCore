using Product.API.Entities;
using Product.API.Data;

namespace Product.API.Persistence
{
    public class ProductContextSeed
    {
        public static async Task SeedAsync(ProductContext context, ILogger<ProductContextSeed> logger)
        {
            // Check if data already exists
            if (context.Products.Any())
            {
                logger.LogInformation("Database already contains products. Skipping seed.");
                return;
            }

            logger.LogInformation("Starting product data seeding from JSON files...");

            // Path to clean folder - can be configured via environment variable
            var dataPath = Environment.GetEnvironmentVariable("SEED_DATA_PATH") 
                ?? "/app/seed_data";  // Default path in Docker container
            
            // Fallback to local development path if not in container
            if (!Directory.Exists(dataPath))
            {
                dataPath = @"c:\Users\PC\Desktop\source\clean";
            }

            if (!Directory.Exists(dataPath))
            {
                logger.LogWarning("Seed data folder not found at {DataPath}. Skipping automatic seeding.", dataPath);
                logger.LogInformation("To seed data manually, run: dotnet run --seed-data");
                return;
            }

            var jsonFiles = Directory.GetFiles(dataPath, "clean_*.json");
            if (!jsonFiles.Any())
            {
                logger.LogWarning("No JSON files found in {DataPath}. Skipping automatic seeding.", dataPath);
                return;
            }

            logger.LogInformation("Found {Count} JSON files in {DataPath}", jsonFiles.Length, dataPath);

            try
            {
                var seeder = new ProductDataSeeder(context, dataPath);
                await seeder.SeedDataAsync();
                
                logger.LogInformation("Product data seeding completed successfully!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during automatic product data seeding: {Message}", ex.Message);
                logger.LogInformation("You can retry seeding manually with: dotnet run --seed-data");
            }
        }

        // Legacy method for backward compatibility
        public static async Task SeedProductAsync(ProductContext context, ILogger<ProductContextSeed> logger)
        {
            await SeedAsync(context, logger);
        }
    }
}
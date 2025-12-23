using Microsoft.EntityFrameworkCore;
using Product.API.Data;
using Product.API.Persistence;

namespace Product.API.Commands
{
    /// <summary>
    /// CLI command to seed product data from JSON files
    /// Usage: dotnet run --seed-data
    /// </summary>
    public static class SeedDataCommand
    {
        public static async Task ExecuteAsync(IServiceProvider serviceProvider, string dataFolderPath)
        {
            Console.WriteLine("=== Product Data Seeding Tool ===\n");

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProductContext>();

            // Ensure database is created and migrated
            Console.WriteLine("Checking database...");
            try
            {
                await context.Database.MigrateAsync();
                Console.WriteLine("Database is up to date.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database migration failed: {ex.Message}");
                Console.WriteLine("Please ensure the database connection string is configured correctly.");
                return;
            }

            // Verify data folder exists
            if (!Directory.Exists(dataFolderPath))
            {
                Console.WriteLine($"Error: Data folder not found: {dataFolderPath}");
                Console.WriteLine("Please provide the correct path to the 'clean' folder containing JSON files.");
                return;
            }

            var jsonFiles = Directory.GetFiles(dataFolderPath, "clean_*.json");
            if (!jsonFiles.Any())
            {
                Console.WriteLine($"Error: No JSON files found in {dataFolderPath}");
                Console.WriteLine("Expected files with pattern: clean_*.json");
                return;
            }

            Console.WriteLine($"Found {jsonFiles.Length} JSON files to process");
            Console.Write("Do you want to proceed with seeding? (y/n): ");
            var confirm = Console.ReadLine();

            if (confirm?.ToLower() != "y")
            {
                Console.WriteLine("Seeding cancelled.");
                return;
            }

            // Run seeder
            var seeder = new ProductDataSeeder(context, dataFolderPath);
            var startTime = DateTime.Now;

            try
            {
                await seeder.SeedDataAsync();
                var duration = DateTime.Now - startTime;
                Console.WriteLine($"\n✓ Seeding completed successfully in {duration.TotalMinutes:F2} minutes!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Seeding failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}

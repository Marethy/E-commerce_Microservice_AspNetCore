using Product.API.Entities;
using Microsoft.Extensions.Logging;

namespace Product.API.Persistence
{
    public class ProductContextSeed
    {
        public static async Task SeedProductAsync(ProductContext context, ILogger<ProductContextSeed> logger)
        {
            if (!context.Products.Any())
            {
                var products = new List<CatalogProduct>
                {
                    new CatalogProduct
                    {
                        No = "P001",
                        Name = "CatalogProduct 1",
                        Summary = "Summary 1",
                        Description = "Description 1",
                        Price = 100,
                        CreatedDate = DateTime.UtcNow
                    },
                    new CatalogProduct
                    {
                        No = "P002",
                        Name = "CatalogProduct 2",
                        Summary = "Summary 2",
                        Description = "Description 2",
                        Price = 200,
                        CreatedDate = DateTime.UtcNow
                    },
                    new CatalogProduct
                    {
                        No = "P003",
                        Name = "CatalogProduct 3",
                        Summary = "Summary 3",
                        Description = "Description 3",
                        Price = 300,
                        CreatedDate = DateTime.UtcNow
                    }
                };
                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded database with initial products.");
            }
        }
    }
}



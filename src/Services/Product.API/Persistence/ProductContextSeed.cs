using Product.API.Entities;

namespace Product.API.Persistence
{
    public class ProductContextSeed
    {
        // Define consistent UUIDs that are PostgreSQL compatible
        private static class SeedIds
        {
            // Category IDs - using standard UUID format
            public static readonly Guid ElectronicsId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001");
            public static readonly Guid ClothingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002");
            public static readonly Guid BooksId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003");
            public static readonly Guid HomeGardenId = Guid.Parse("550e8400-e29b-41d4-a716-446655440004");
            public static readonly Guid SportsId = Guid.Parse("550e8400-e29b-41d4-a716-446655440005");

            // Product IDs - using standard UUID format
            public static readonly Guid SmartphoneId = Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
            public static readonly Guid EarbudsId = Guid.Parse("6ba7b811-9dad-11d1-80b4-00c04fd430c8");
            public static readonly Guid LaptopId = Guid.Parse("6ba7b812-9dad-11d1-80b4-00c04fd430c8");
            public static readonly Guid DenimJacketId = Guid.Parse("6ba7b813-9dad-11d1-80b4-00c04fd430c8");
            public static readonly Guid RunningShoesId = Guid.Parse("6ba7b814-9dad-11d1-80b4-00c04fd430c8");
            public static readonly Guid CleanCodeBookId = Guid.Parse("6ba7b815-9dad-11d1-80b4-00c04fd430c8");
            public static readonly Guid DesignPatternsBookId = Guid.Parse("6ba7b816-9dad-11d1-80b4-00c04fd430c8");
            public static readonly Guid SmartHomeHubId = Guid.Parse("6ba7b817-9dad-11d1-80b4-00c04fd430c8");
            public static readonly Guid YogaMatId = Guid.Parse("6ba7b818-9dad-11d1-80b4-00c04fd430c8");

            // Review IDs - using full UUID format
            public static readonly Guid Review01Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479");
            public static readonly Guid Review02Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d480");
            public static readonly Guid Review03Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d481");
            public static readonly Guid Review04Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d482");
            public static readonly Guid Review05Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d483");
            public static readonly Guid Review06Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d484");
            public static readonly Guid Review07Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d485");
            public static readonly Guid Review08Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d486");
            public static readonly Guid Review09Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d487");
            public static readonly Guid Review10Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d488");
            public static readonly Guid Review11Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d489");
            public static readonly Guid Review12Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d490");
            public static readonly Guid Review13Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d491");
        }

        public static async Task SeedAsync(ProductContext context, ILogger<ProductContextSeed> logger)
        {
            await SeedCategoriesAsync(context, logger);
            await SeedProductsAsync(context, logger);
            await SeedProductReviewsAsync(context, logger);
        }

        private static async Task SeedCategoriesAsync(ProductContext context, ILogger<ProductContextSeed> logger)
        {
            if (!context.Categories.Any())
            {
                logger.LogInformation("Seeding database with initial categories...");

                var categories = new List<Category>
                {
                    new Category
                    {
                        Id = SeedIds.ElectronicsId,
                        Name = "Electronics",
                        Description = "Electronic devices and accessories"
                    },
                    new Category
                    {
                        Id = SeedIds.ClothingId,
                        Name = "Clothing",
                        Description = "Fashion and apparel items"
                    },
                    new Category
                    {
                        Id = SeedIds.BooksId,
                        Name = "Books",
                        Description = "Books and educational materials"
                    },
                    new Category
                    {
                        Id = SeedIds.HomeGardenId,
                        Name = "Home & Garden",
                        Description = "Home improvement and garden supplies"
                    },
                    new Category
                    {
                        Id = SeedIds.SportsId,
                        Name = "Sports",
                        Description = "Sports equipment and accessories"
                    }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                logger.LogInformation("Seeded database with {CategoryCount} initial categories.", categories.Count);
            }
            else
            {
                logger.LogInformation("Database already contains categories. No seeding required.");
            }
        }

        private static async Task SeedProductsAsync(ProductContext context, ILogger<ProductContextSeed> logger)
        {
            if (!context.Products.Any())
            {
                logger.LogInformation("Seeding database with initial products...");

                var products = new List<CatalogProduct>
                {
                    // Electronics
                    new CatalogProduct
                    {
                        Id = SeedIds.SmartphoneId,
                        No = "ELE001",
                        Name = "Smartphone Pro Max",
                        Summary = "Latest flagship smartphone with advanced features",
                        Description = "High-performance smartphone with 6.7-inch display, triple camera system, and 5G connectivity. Perfect for professionals and tech enthusiasts.",
                        Price = 999.99m,
                        CategoryId = SeedIds.ElectronicsId
                    },
                    new CatalogProduct
                    {
                        Id = SeedIds.EarbudsId,
                        No = "ELE002", 
                        Name = "Wireless Earbuds",
                        Summary = "Premium noise-cancelling wireless earbuds",
                        Description = "High-quality wireless earbuds with active noise cancellation, 24-hour battery life, and premium sound quality.",
                        Price = 199.99m,
                        CategoryId = SeedIds.ElectronicsId
                    },
                    new CatalogProduct
                    {
                        Id = SeedIds.LaptopId,
                        No = "ELE003",
                        Name = "Gaming Laptop",
                        Summary = "High-performance gaming laptop",
                        Description = "Powerful gaming laptop with RTX graphics, 16GB RAM, and 1TB SSD. Perfect for gaming and content creation.",
                        Price = 1599.99m,
                        CategoryId = SeedIds.ElectronicsId
                    },

                    // Clothing
                    new CatalogProduct
                    {
                        Id = SeedIds.DenimJacketId,
                        No = "CLO001",
                        Name = "Classic Denim Jacket",
                        Summary = "Timeless denim jacket for casual wear",
                        Description = "100% cotton denim jacket with classic fit. Perfect for layering and casual outings.",
                        Price = 79.99m,
                        CategoryId = SeedIds.ClothingId
                    },
                    new CatalogProduct
                    {
                        Id = SeedIds.RunningShoesId,
                        No = "CLO002",
                        Name = "Running Shoes",
                        Summary = "Comfortable running shoes for daily exercise",
                        Description = "Lightweight running shoes with cushioned sole and breathable mesh upper. Ideal for jogging and fitness activities.",
                        Price = 129.99m,
                        CategoryId = SeedIds.ClothingId
                    },

                    // Books
                    new CatalogProduct
                    {
                        Id = SeedIds.CleanCodeBookId,
                        No = "BOO001",
                        Name = "Clean Code: A Handbook",
                        Summary = "Essential guide for software craftsmanship",
                        Description = "Learn the principles of writing clean, maintainable code. A must-read for every software developer.",
                        Price = 49.99m,
                        CategoryId = SeedIds.BooksId
                    },
                    new CatalogProduct
                    {
                        Id = SeedIds.DesignPatternsBookId,
                        No = "BOO002",
                        Name = "Design Patterns",
                        Summary = "Elements of reusable object-oriented software",
                        Description = "Classic book on software design patterns. Essential reading for software architects and developers.",
                        Price = 59.99m,
                        CategoryId = SeedIds.BooksId
                    },

                    // Home & Garden
                    new CatalogProduct
                    {
                        Id = SeedIds.SmartHomeHubId,
                        No = "HOM001",
                        Name = "Smart Home Hub",
                        Summary = "Central control for smart home devices",
                        Description = "Control all your smart home devices from one central hub. Compatible with major smart home platforms.",
                        Price = 149.99m,
                        CategoryId = SeedIds.HomeGardenId
                    },

                    // Sports
                    new CatalogProduct
                    {
                        Id = SeedIds.YogaMatId,
                        No = "SPO001",
                        Name = "Yoga Mat Premium",
                        Summary = "Non-slip yoga mat for all fitness levels",
                        Description = "High-quality yoga mat with superior grip and cushioning. Perfect for yoga, pilates, and fitness workouts.",
                        Price = 39.99m,
                        CategoryId = SeedIds.SportsId
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();

                logger.LogInformation("Seeded database with {ProductCount} initial products.", products.Count);
            }
            else
            {
                logger.LogInformation("Database already contains products. No seeding required.");
            }
        }

        private static async Task SeedProductReviewsAsync(ProductContext context, ILogger<ProductContextSeed> logger)
        {
            if (!context.ProductReviews.Any())
            {
                logger.LogInformation("Seeding database with initial product reviews...");

                var reviews = new List<ProductReview>
                {
                    // Reviews for Smartphone Pro Max
                    new ProductReview
                    {
                        Id = SeedIds.Review01Id,
                        ProductId = SeedIds.SmartphoneId,
                        UserId = "user001@example.com",
                        Rating = 5,
                        Comment = "Excellent phone! Great camera quality and battery life."
                    },
                    new ProductReview
                    {
                        Id = SeedIds.Review02Id,
                        ProductId = SeedIds.SmartphoneId,
                        UserId = "user002@example.com",
                        Rating = 4,
                        Comment = "Good performance but a bit expensive."
                    },

                    // Reviews for Wireless Earbuds
                    new ProductReview
                    {
                        Id = SeedIds.Review03Id,
                        ProductId = SeedIds.EarbudsId,
                        UserId = "user003@example.com",
                        Rating = 5,
                        Comment = "Amazing sound quality and noise cancellation!"
                    },
                    new ProductReview
                    {
                        Id = SeedIds.Review04Id,
                        ProductId = SeedIds.EarbudsId,
                        UserId = "user004@example.com",
                        Rating = 4,
                        Comment = "Very comfortable to wear for long periods."
                    },

                    // Reviews for Gaming Laptop
                    new ProductReview
                    {
                        Id = SeedIds.Review05Id,
                        ProductId = SeedIds.LaptopId,
                        UserId = "user005@example.com",
                        Rating = 5,
                        Comment = "Perfect for gaming and streaming. Highly recommended!"
                    },

                    // Reviews for Classic Denim Jacket
                    new ProductReview
                    {
                        Id = SeedIds.Review06Id,
                        ProductId = SeedIds.DenimJacketId,
                        UserId = "user006@example.com",
                        Rating = 4,
                        Comment = "Great quality denim, fits perfectly."
                    },

                    // Reviews for Running Shoes
                    new ProductReview
                    {
                        Id = SeedIds.Review07Id,
                        ProductId = SeedIds.RunningShoesId,
                        UserId = "user007@example.com",
                        Rating = 5,
                        Comment = "Very comfortable for daily runs. Great cushioning!"
                    },
                    new ProductReview
                    {
                        Id = SeedIds.Review08Id,
                        ProductId = SeedIds.RunningShoesId,
                        UserId = "user008@example.com",
                        Rating = 4,
                        Comment = "Good shoes but took some time to break in."
                    },

                    // Reviews for Clean Code book
                    new ProductReview
                    {
                        Id = SeedIds.Review09Id,
                        ProductId = SeedIds.CleanCodeBookId,
                        UserId = "developer001@example.com",
                        Rating = 5,
                        Comment = "Must-read for every developer. Changed how I write code!"
                    },

                    // Reviews for Design Patterns book
                    new ProductReview
                    {
                        Id = SeedIds.Review10Id,
                        ProductId = SeedIds.DesignPatternsBookId,
                        UserId = "architect001@example.com",
                        Rating = 5,
                        Comment = "Classic book with timeless patterns. Excellent resource!"
                    },

                    // Reviews for Smart Home Hub
                    new ProductReview
                    {
                        Id = SeedIds.Review11Id,
                        ProductId = SeedIds.SmartHomeHubId,
                        UserId = "homeowner001@example.com",
                        Rating = 4,
                        Comment = "Easy to set up and works well with all my devices."
                    },

                    // Reviews for Yoga Mat
                    new ProductReview
                    {
                        Id = SeedIds.Review12Id,
                        ProductId = SeedIds.YogaMatId,
                        UserId = "yogi001@example.com",
                        Rating = 5,
                        Comment = "Excellent grip and cushioning. Perfect for yoga practice."
                    },
                    new ProductReview
                    {
                        Id = SeedIds.Review13Id,
                        ProductId = SeedIds.YogaMatId,
                        UserId = "fitness001@example.com",
                        Rating = 4,
                        Comment = "Good quality mat, very durable."
                    }
                };

                await context.ProductReviews.AddRangeAsync(reviews);
                await context.SaveChangesAsync();

                logger.LogInformation("Seeded database with {ReviewCount} initial product reviews.", reviews.Count);
            }
            else
            {
                logger.LogInformation("Database already contains product reviews. No seeding required.");
            }
        }

        // Legacy method for backward compatibility
        public static async Task SeedProductAsync(ProductContext context, ILogger<ProductContextSeed> logger)
        {
            await SeedAsync(context, logger);
        }
    }
}
using Product.API.Entities;

namespace Product.API.Persistence
{
    public class ProductContextSeed
    {
        // Define consistent UUIDs that are PostgreSQL compatible
        private static class SeedIds
        {
            // Category IDs
            public static readonly Guid ElectronicsId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001");
            public static readonly Guid ClothingId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002");
            public static readonly Guid BooksId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003");
            public static readonly Guid HomeGardenId = Guid.Parse("550e8400-e29b-41d4-a716-446655440004");
            public static readonly Guid SportsId = Guid.Parse("550e8400-e29b-41d4-a716-446655440005");

            // Brand IDs
            public static readonly Guid TechPlusId = Guid.Parse("650e8400-e29b-41d4-a716-446655440001");
            public static readonly Guid SportWearId = Guid.Parse("650e8400-e29b-41d4-a716-446655440002");
            public static readonly Guid OReillybooksId = Guid.Parse("650e8400-e29b-41d4-a716-446655440003");
            public static readonly Guid SmartHomeCoid = Guid.Parse("650e8400-e29b-41d4-a716-446655440004");

            // Seller IDs
            public static readonly Guid OfficialStoreId = Guid.Parse("750e8400-e29b-41d4-a716-446655440001");
            public static readonly Guid MarketplaceVendorId = Guid.Parse("750e8400-e29b-41d4-a716-446655440002");

            // Product IDs
            public static readonly Guid SmartphoneId = Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
            public static readonly Guid EarbudsId = Guid.Parse("6ba7b811-9dad-11d1-80b4-00c04fd430c8");
            public static readonly Guid LaptopId = Guid.Parse("6ba7b812-9dad-11d1-80b4-00c04fd430c8");
            public static readonly Guid DenimJacketId = Guid.Parse("6ba7b813-9dad-11d1-80b4-00c04fd430c8");
            public static readonly Guid RunningShoesId = Guid.Parse("6ba7b814-9dad-11d1-80b4-00c04fd430c8");
            public static readonly Guid CleanCodeBookId = Guid.Parse("6ba7b815-9dad-11d1-80b4-00c04fd430c8");
            public static readonly Guid DesignPatternsBookId = Guid.Parse("6ba7b816-9dad-11d1-80b4-00c04fd430c8");
            public static readonly Guid SmartHomeHubId = Guid.Parse("6ba7b817-9dad-11d1-80b4-00c04fd430c8");
            public static readonly Guid YogaMatId = Guid.Parse("6ba7b818-9dad-11d1-80b4-00c04fd430c8");

            // Review IDs
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
            await SeedBrandsAsync(context, logger);
            await SeedSellersAsync(context, logger);
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
                    new Category { Id = SeedIds.ElectronicsId, Name = "Electronics", Description = "Electronic devices and accessories" },
                    new Category { Id = SeedIds.ClothingId, Name = "Clothing", Description = "Fashion and apparel items" },
                    new Category { Id = SeedIds.BooksId, Name = "Books", Description = "Books and educational materials" },
                    new Category { Id = SeedIds.HomeGardenId, Name = "Home & Garden", Description = "Home improvement and garden supplies" },
                    new Category { Id = SeedIds.SportsId, Name = "Sports", Description = "Sports equipment and accessories" }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} categories", categories.Count);
            }
        }

        private static async Task SeedBrandsAsync(ProductContext context, ILogger<ProductContextSeed> logger)
        {
            if (!context.Brands.Any())
            {
                logger.LogInformation("Seeding database with initial brands...");

                var brands = new List<Brand>
                {
                    new Brand 
                    { 
                        Id = SeedIds.TechPlusId, 
                        Name = "TechPlus", 
                        Slug = "techplus",
                        CountryOfOrigin = "USA",
                        Description = "Leading technology brand for smartphones and electronics"
                    },
                    new Brand 
                    { 
                        Id = SeedIds.SportWearId, 
                        Name = "SportWear Pro", 
                        Slug = "sportwear-pro",
                        CountryOfOrigin = "Germany",
                        Description = "Premium sports clothing and equipment"
                    },
                    new Brand 
                    { 
                        Id = SeedIds.OReillybooksId, 
                        Name = "O'Reilly Media", 
                        Slug = "oreilly-media",
                        CountryOfOrigin = "USA",
                        Description = "Leading publisher of technology and business books"
                    },
                    new Brand 
                    { 
                        Id = SeedIds.SmartHomeCoid, 
                        Name = "SmartHome Co", 
                        Slug = "smarthome-co",
                        CountryOfOrigin = "Japan",
                        Description = "Innovative smart home devices and solutions"
                    }
                };

                await context.Brands.AddRangeAsync(brands);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} brands", brands.Count);
            }
        }

        private static async Task SeedSellersAsync(ProductContext context, ILogger<ProductContextSeed> logger)
        {
            if (!context.Sellers.Any())
            {
                logger.LogInformation("Seeding database with initial sellers...");

                var sellers = new List<Seller>
                {
                    new Seller 
                    { 
                        Id = SeedIds.OfficialStoreId, 
                        Name = "Official Store", 
                        IsOfficial = true,
                        Email = "official@store.com",
                        Rating = 4.8m,
                        TotalSales = 10000
                    },
                    new Seller 
                    { 
                        Id = SeedIds.MarketplaceVendorId, 
                        Name = "Marketplace Vendor", 
                        IsOfficial = false,
                        Email = "vendor@marketplace.com",
                        Rating = 4.5m,
                        TotalSales = 5000
                    }
                };

                await context.Sellers.AddRangeAsync(sellers);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} sellers", sellers.Count);
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
                        Description = "High-performance smartphone with 6.7-inch display, triple camera system, and 5G connectivity.",
                        ShortDescription = "Premium flagship smartphone",
                        Price = 999.99m,
                        OriginalPrice = 1199.99m,
                        DiscountPercentage = 17,
                        Slug = "smartphone-pro-max",
                        CategoryId = SeedIds.ElectronicsId,
                        BrandId = SeedIds.TechPlusId,
                        SellerId = SeedIds.OfficialStoreId,
                        StockQuantity = 50,
                        InventoryStatus = "IN_STOCK",
                        AllTimeQuantitySold = 500,
                        QuantitySoldLast30Days = 45
                    },
                    new CatalogProduct
                    {
                        Id = SeedIds.EarbudsId,
                        No = "ELE002", 
                        Name = "Wireless Earbuds",
                        Summary = "Premium noise-cancelling wireless earbuds",
                        Description = "High-quality wireless earbuds with active noise cancellation, 24-hour battery life.",
                        ShortDescription = "Premium wireless earbuds",
                        Price = 199.99m,
                        OriginalPrice = 249.99m,
                        DiscountPercentage = 20,
                        Slug = "wireless-earbuds",
                        CategoryId = SeedIds.ElectronicsId,
                        BrandId = SeedIds.TechPlusId,
                        SellerId = SeedIds.OfficialStoreId,
                        StockQuantity = 100,
                        InventoryStatus = "IN_STOCK",
                        AllTimeQuantitySold = 800,
                        QuantitySoldLast30Days = 75
                    },
                    new CatalogProduct
                    {
                        Id = SeedIds.LaptopId,
                        No = "ELE003",
                        Name = "Gaming Laptop",
                        Summary = "High-performance gaming laptop",
                        Description = "Powerful gaming laptop with RTX graphics, 16GB RAM, and 1TB SSD.",
                        ShortDescription = "Gaming powerhouse",
                        Price = 1599.99m,
                        Slug = "gaming-laptop",
                        CategoryId = SeedIds.ElectronicsId,
                        BrandId = SeedIds.TechPlusId,
                        SellerId = SeedIds.OfficialStoreId,
                        StockQuantity = 25,
                        InventoryStatus = "IN_STOCK",
                        AllTimeQuantitySold = 150,
                        QuantitySoldLast30Days = 12
                    },

                    // Clothing & Sports
                    new CatalogProduct
                    {
                        Id = SeedIds.DenimJacketId,
                        No = "CLO001",
                        Name = "Classic Denim Jacket",
                        Summary = "Timeless denim jacket for casual wear",
                        Description = "100% cotton denim jacket with classic fit.",
                        ShortDescription = "Classic denim style",
                        Price = 79.99m,
                        Slug = "classic-denim-jacket",
                        CategoryId = SeedIds.ClothingId,
                        SellerId = SeedIds.MarketplaceVendorId,
                        StockQuantity = 200,
                        InventoryStatus = "IN_STOCK",
                        AllTimeQuantitySold = 350,
                        QuantitySoldLast30Days = 28
                    },
                    new CatalogProduct
                    {
                        Id = SeedIds.RunningShoesId,
                        No = "CLO002",
                        Name = "Running Shoes",
                        Summary = "Comfortable running shoes for daily exercise",
                        Description = "Lightweight running shoes with cushioned sole and breathable mesh upper.",
                        ShortDescription = "Premium running shoes",
                        Price = 129.99m,
                        Slug = "running-shoes",
                        CategoryId = SeedIds.ClothingId,
                        BrandId = SeedIds.SportWearId,
                        SellerId = SeedIds.OfficialStoreId,
                        StockQuantity = 150,
                        InventoryStatus = "IN_STOCK",
                        AllTimeQuantitySold = 450,
                        QuantitySoldLast30Days = 40
                    },

                    // Books
                    new CatalogProduct
                    {
                        Id = SeedIds.CleanCodeBookId,
                        No = "BOO001",
                        Name = "Clean Code: A Handbook",
                        Summary = "Essential guide for software craftsmanship",
                        Description = "Learn the principles of writing clean, maintainable code.",
                        ShortDescription = "Software craftsmanship guide",
                        Price = 49.99m,
                        Slug = "clean-code-handbook",
                        CategoryId = SeedIds.BooksId,
                        BrandId = SeedIds.OReillybooksId,
                        SellerId = SeedIds.MarketplaceVendorId,
                        StockQuantity = 300,
                        InventoryStatus = "IN_STOCK",
                        AllTimeQuantitySold = 1200,
                        QuantitySoldLast30Days = 95
                    },
                    new CatalogProduct
                    {
                        Id = SeedIds.DesignPatternsBookId,
                        No = "BOO002",
                        Name = "Design Patterns",
                        Summary = "Elements of reusable object-oriented software",
                        Description = "Classic book on software design patterns.",
                        ShortDescription = "Design patterns bible",
                        Price = 59.99m,
                        Slug = "design-patterns",
                        CategoryId = SeedIds.BooksId,
                        BrandId = SeedIds.OReillybooksId,
                        SellerId = SeedIds.MarketplaceVendorId,
                        StockQuantity = 250,
                        InventoryStatus = "IN_STOCK",
                        AllTimeQuantitySold = 900,
                        QuantitySoldLast30Days = 68
                    },

                    // Home & Garden
                    new CatalogProduct
                    {
                        Id = SeedIds.SmartHomeHubId,
                        No = "HOM001",
                        Name = "Smart Home Hub",
                        Summary = "Central control for smart home devices",
                        Description = "Control all your smart home devices from one central hub.",
                        ShortDescription = "Smart home control center",
                        Price = 149.99m,
                        Slug = "smart-home-hub",
                        CategoryId = SeedIds.HomeGardenId,
                        BrandId = SeedIds.SmartHomeCoid,
                        SellerId = SeedIds.OfficialStoreId,
                        StockQuantity = 75,
                        InventoryStatus = "IN_STOCK",
                        AllTimeQuantitySold = 300,
                        QuantitySoldLast30Days = 25
                    },

                    // Sports
                    new CatalogProduct
                    {
                        Id = SeedIds.YogaMatId,
                        No = "SPO001",
                        Name = "Yoga Mat Premium",
                        Summary = "Non-slip yoga mat for all fitness levels",
                        Description = "High-quality yoga mat with superior grip and cushioning.",
                        ShortDescription = "Premium yoga mat",
                        Price = 39.99m,
                        Slug = "yoga-mat-premium",
                        CategoryId = SeedIds.SportsId,
                        BrandId = SeedIds.SportWearId,
                        SellerId = SeedIds.OfficialStoreId,
                        StockQuantity = 500,
                        InventoryStatus = "IN_STOCK",
                        AllTimeQuantitySold = 850,
                        QuantitySoldLast30Days = 72
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} products", products.Count);
            }
        }

        private static async Task SeedProductReviewsAsync(ProductContext context, ILogger<ProductContextSeed> logger)
        {
            if (!context.ProductReviews.Any())
            {
                logger.LogInformation("Seeding database with initial product reviews...");

                var reviews = new List<ProductReview>
                {
                    // Smartphone reviews
                    new ProductReview { Id = SeedIds.Review01Id, ProductId = SeedIds.SmartphoneId, UserId = "user001@example.com", Rating = 5.0m, Title = "Excellent phone!", Comment = "Great camera quality and battery life.", VerifiedPurchase = true, ReviewDate = DateTimeOffset.UtcNow.AddDays(-10) },
                    new ProductReview { Id = SeedIds.Review02Id, ProductId = SeedIds.SmartphoneId, UserId = "user002@example.com", Rating = 4.0m, Title = "Good but pricey", Comment = "Good performance but a bit expensive.", VerifiedPurchase = true, ReviewDate = DateTimeOffset.UtcNow.AddDays(-8) },
                    
                    // Earbuds reviews
                    new ProductReview { Id = SeedIds.Review03Id, ProductId = SeedIds.EarbudsId, UserId = "user003@example.com", Rating = 5.0m, Title = "Amazing sound!", Comment = "Amazing sound quality and noise cancellation!", VerifiedPurchase = true, ReviewDate = DateTimeOffset.UtcNow.AddDays(-15) },
                    new ProductReview { Id = SeedIds.Review04Id, ProductId = SeedIds.EarbudsId, UserId = "user004@example.com", Rating = 4.5m, Title = "Very comfortable", Comment = "Very comfortable to wear for long periods.", VerifiedPurchase = true, ReviewDate = DateTimeOffset.UtcNow.AddDays(-12) },
                    
                    // Other reviews
                    new ProductReview { Id = SeedIds.Review05Id, ProductId = SeedIds.LaptopId, UserId = "user005@example.com", Rating = 5.0m, Title = "Perfect for gaming", Comment = "Perfect for gaming and streaming. Highly recommended!", VerifiedPurchase = true, ReviewDate = DateTimeOffset.UtcNow.AddDays(-20) },
                    new ProductReview { Id = SeedIds.Review06Id, ProductId = SeedIds.DenimJacketId, UserId = "user006@example.com", Rating = 4.0m, Title = "Great quality", Comment = "Great quality denim, fits perfectly.", VerifiedPurchase = false, ReviewDate = DateTimeOffset.UtcNow.AddDays(-5) },
                    new ProductReview { Id = SeedIds.Review07Id, ProductId = SeedIds.RunningShoesId, UserId = "user007@example.com", Rating = 5.0m, Title = "Perfect shoes!", Comment = "Very comfortable for daily runs. Great cushioning!", VerifiedPurchase = true, ReviewDate = DateTimeOffset.UtcNow.AddDays(-18) },
                    new ProductReview { Id = SeedIds.Review08Id, ProductId = SeedIds.RunningShoesId, UserId = "user008@example.com", Rating = 4.0m, Title = "Good shoes", Comment = "Good shoes but took some time to break in.", VerifiedPurchase = true, ReviewDate = DateTimeOffset.UtcNow.AddDays(-14) },
                    new ProductReview { Id = SeedIds.Review09Id, ProductId = SeedIds.CleanCodeBookId, UserId = "developer001@example.com", Rating = 5.0m, Title = "Must-read!", Comment = "Must-read for every developer. Changed how I write code!", VerifiedPurchase = true, ReviewDate = DateTimeOffset.UtcNow.AddDays(-30) },
                    new ProductReview { Id = SeedIds.Review10Id, ProductId = SeedIds.DesignPatternsBookId, UserId = "architect001@example.com", Rating = 5.0m, Title = "Classic!", Comment = "Classic book with timeless patterns. Excellent resource!", VerifiedPurchase = true, ReviewDate = DateTimeOffset.UtcNow.AddDays(-25) },
                    new ProductReview { Id = SeedIds.Review11Id, ProductId = SeedIds.SmartHomeHubId, UserId = "homeowner001@example.com", Rating = 4.5m, Title = "Easy setup", Comment = "Easy to set up and works well with all my devices.", VerifiedPurchase = true, ReviewDate = DateTimeOffset.UtcNow.AddDays(-22) },
                    new ProductReview { Id = SeedIds.Review12Id, ProductId = SeedIds.YogaMatId, UserId = "yogi001@example.com", Rating = 5.0m, Title = "Perfect mat!", Comment = "Excellent grip and cushioning. Perfect for yoga practice.", VerifiedPurchase = true, ReviewDate = DateTimeOffset.UtcNow.AddDays(-7) },
                    new ProductReview { Id = SeedIds.Review13Id, ProductId = SeedIds.YogaMatId, UserId = "fitness001@example.com", Rating = 4.5m, Title = "Durable", Comment = "Good quality mat, very durable.", VerifiedPurchase = false, ReviewDate = DateTimeOffset.UtcNow.AddDays(-3) }
                };

                await context.ProductReviews.AddRangeAsync(reviews);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} product reviews", reviews.Count);
            }
        }

        // Legacy method for backward compatibility
        public static async Task SeedProductAsync(ProductContext context, ILogger<ProductContextSeed> logger)
        {
            await SeedAsync(context, logger);
        }
    }
}
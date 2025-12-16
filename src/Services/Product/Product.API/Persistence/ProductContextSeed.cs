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

            // Image IDs
            public static readonly Guid Img01Id = Guid.Parse("a47ac10b-58cc-4372-a567-0e02b2c3d501");
            public static readonly Guid Img02Id = Guid.Parse("a47ac10b-58cc-4372-a567-0e02b2c3d502");
            public static readonly Guid Img03Id = Guid.Parse("a47ac10b-58cc-4372-a567-0e02b2c3d503");
            public static readonly Guid Img04Id = Guid.Parse("a47ac10b-58cc-4372-a567-0e02b2c3d504");
            public static readonly Guid Img05Id = Guid.Parse("a47ac10b-58cc-4372-a567-0e02b2c3d505");
            public static readonly Guid Img06Id = Guid.Parse("a47ac10b-58cc-4372-a567-0e02b2c3d506");
            public static readonly Guid Img07Id = Guid.Parse("a47ac10b-58cc-4372-a567-0e02b2c3d507");
            public static readonly Guid Img08Id = Guid.Parse("a47ac10b-58cc-4372-a567-0e02b2c3d508");
            public static readonly Guid Img09Id = Guid.Parse("a47ac10b-58cc-4372-a567-0e02b2c3d509");
        }

        public static async Task SeedAsync(ProductContext context, ILogger<ProductContextSeed> logger)
        {
            await SeedCategoriesAsync(context, logger);
            await SeedBrandsAsync(context, logger);
            await SeedSellersAsync(context, logger);
            await SeedProductsAsync(context, logger);
            await SeedProductImagesAsync(context, logger);
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
                        Price = 24499755m,        // 999.99 USD = 24,499,755 VND
                        OriginalPrice = 29399755m, // 1199.99 USD = 29,399,755 VND
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
                        Price = 4899755m,         // 199.99 USD = 4,899,755 VND
                        OriginalPrice = 6124755m, // 249.99 USD = 6,124,755 VND
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
                        Price = 39199755m,        // 1599.99 USD = 39,199,755 VND
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
                        Price = 1959755m,         // 79.99 USD = 1,959,755 VND
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
                        Price = 3184755m,         // 129.99 USD = 3,184,755 VND
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
                        Price = 1224755m,         // 49.99 USD = 1,224,755 VND
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
                        Price = 1469755m,         // 59.99 USD = 1,469,755 VND
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
                        Price = 3674755m,         // 149.99 USD = 3,674,755 VND
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
                        Price = 979755m,          // 39.99 USD = 979,755 VND
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

        private static async Task SeedProductImagesAsync(ProductContext context, ILogger<ProductContextSeed> logger)
        {
            if (!context.ProductImages.Any())
            {
                logger.LogInformation("Seeding database with initial product images...");

                var images = new List<ProductImage>
                {
                    // Smartphone images
                    new ProductImage { Id = SeedIds.Img01Id, ProductId = SeedIds.SmartphoneId, Url = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=600", AltText = "Smartphone Pro X - Front View", Position = 1, IsPrimary = true },
                    
                    // Earbuds images
                    new ProductImage { Id = SeedIds.Img02Id, ProductId = SeedIds.EarbudsId, Url = "https://images.unsplash.com/photo-1590658268037-6bf12165a8df?w=600", AltText = "Wireless Earbuds Pro", Position = 1, IsPrimary = true },
                    
                    // Laptop images
                    new ProductImage { Id = SeedIds.Img03Id, ProductId = SeedIds.LaptopId, Url = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=600", AltText = "Gaming Laptop Ultra", Position = 1, IsPrimary = true },
                    
                    // Denim Jacket images
                    new ProductImage { Id = SeedIds.Img04Id, ProductId = SeedIds.DenimJacketId, Url = "https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600", AltText = "Classic Denim Jacket", Position = 1, IsPrimary = true },
                    
                    // Running Shoes images
                    new ProductImage { Id = SeedIds.Img05Id, ProductId = SeedIds.RunningShoesId, Url = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=600", AltText = "Running Shoes Max", Position = 1, IsPrimary = true },
                    
                    // Clean Code book images
                    new ProductImage { Id = SeedIds.Img06Id, ProductId = SeedIds.CleanCodeBookId, Url = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=600", AltText = "Clean Code Book", Position = 1, IsPrimary = true },
                    
                    // Design Patterns book images
                    new ProductImage { Id = SeedIds.Img07Id, ProductId = SeedIds.DesignPatternsBookId, Url = "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=600", AltText = "Design Patterns Book", Position = 1, IsPrimary = true },
                    
                    // Smart Home Hub images
                    new ProductImage { Id = SeedIds.Img08Id, ProductId = SeedIds.SmartHomeHubId, Url = "https://images.unsplash.com/photo-1558089687-f282ffcbc126?w=600", AltText = "Smart Home Hub", Position = 1, IsPrimary = true },
                    
                    // Yoga Mat images
                    new ProductImage { Id = SeedIds.Img09Id, ProductId = SeedIds.YogaMatId, Url = "https://images.unsplash.com/photo-1601925260368-ae2f83cf8b7f?w=600", AltText = "Premium Yoga Mat", Position = 1, IsPrimary = true }
                };

                await context.ProductImages.AddRangeAsync(images);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} product images", images.Count);
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
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Product.API.Entities;
using Product.API.Persistence;
using Product.API.Services.Interfaces;

namespace Product.API.Data
{
    public class ProductDataSeeder
    {
        private readonly ProductContext _context;
        private readonly string _dataFolderPath;
        private readonly IClipSearchService? _clipSearchService;

        public ProductDataSeeder(ProductContext context, string dataFolderPath, IClipSearchService? clipSearchService = null)
        {
            _context = context;
            _dataFolderPath = dataFolderPath;
            _clipSearchService = clipSearchService;
        }

        public async Task SeedDataAsync()
        {
            Console.WriteLine("Starting product data seeding...");

            var brandCache = new Dictionary<string, Guid>();
            var brandSlugCounter = new Dictionary<string, int>();
            var sellerCache = new Dictionary<string, Guid>();
            var categoryCache = new Dictionary<string, Guid>();
            
            var existingCategories = await _context.Categories.ToListAsync();
            foreach (var cat in existingCategories)
            {
                categoryCache[cat.Name] = cat.Id;
            }
            Console.WriteLine($"Loaded {existingCategories.Count} existing categories into cache");
            
            int negativeExternalId = -1;

            var jsonFiles = Directory.GetFiles(_dataFolderPath, "clean_*.json");
            Console.WriteLine($"Found {jsonFiles.Length} JSON files to process");

            int totalProducts = 0;
            int skippedProducts = 0;

            foreach (var jsonFile in jsonFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(jsonFile);
                var categoryName = fileName.Replace("clean_", "");
                
                Console.WriteLine($"Processing file: {fileName}");

                var jsonContent = await File.ReadAllTextAsync(jsonFile);
                var products = JsonSerializer.Deserialize<List<JsonProduct>>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (products == null)
                {
                    Console.WriteLine($"Failed to parse {fileName}");
                    continue;
                }

                var validProducts = products.Where(p => p.Price.HasValue && p.Price > 0).ToList();
                Console.WriteLine($"  Valid products: {validProducts.Count}/{products.Count}");

                foreach (var jsonProduct in validProducts)
                {
                    try
                    {
                        var existingProduct = await _context.Products
                            .FirstOrDefaultAsync(p => p.ExternalId == jsonProduct.Id);

                        if (existingProduct != null)
                        {
                            skippedProducts++;
                            continue;
                        }

                        Guid? brandId = null;
                        if (jsonProduct.Brand != null && !string.IsNullOrEmpty(jsonProduct.Brand.Name))
                        {
                            if (brandCache.TryGetValue(jsonProduct.Brand.Name, out var cachedBrandId))
                            {
                                brandId = cachedBrandId;
                            }
                            else
                            {
                                var existingBrand = await _context.Brands
                                    .FirstOrDefaultAsync(b => b.Name == jsonProduct.Brand.Name);

                                if (existingBrand != null)
                                {
                                    brandId = existingBrand.Id;
                                    brandCache[jsonProduct.Brand.Name] = brandId.Value;
                                    if (jsonProduct.Brand.Id > 0 && existingBrand.ExternalId == null)
                                    {
                                        existingBrand.ExternalId = jsonProduct.Brand.Id;
                                    }
                                }
                                else
                                {
                                    var baseSlug = jsonProduct.Brand.Slug ?? jsonProduct.Brand.Name.ToLower().Replace(" ", "-");
                                    var slug = baseSlug;
                                    
                                    if (brandSlugCounter.ContainsKey(baseSlug))
                                    {
                                        brandSlugCounter[baseSlug]++;
                                        slug = $"{baseSlug}-{brandSlugCounter[baseSlug]}";
                                    }
                                    else
                                    {
                                        var existingSlug = await _context.Brands.AnyAsync(b => b.Slug == slug);
                                        if (existingSlug)
                                        {
                                            brandSlugCounter[baseSlug] = 1;
                                            slug = $"{baseSlug}-1";
                                        }
                                        else
                                        {
                                            brandSlugCounter[baseSlug] = 0;
                                        }
                                    }
                                    
                                    var newBrand = new Brand
                                    {
                                        Id = Guid.NewGuid(),
                                        ExternalId = jsonProduct.Brand.Id > 0 ? jsonProduct.Brand.Id : null,
                                        Name = jsonProduct.Brand.Name,
                                        Slug = slug
                                    };
                                    _context.Brands.Add(newBrand);
                                    brandId = newBrand.Id;
                                    brandCache[jsonProduct.Brand.Name] = brandId.Value;
                                }
                            }
                        }

                        Guid? sellerId = null;
                        if (jsonProduct.Seller != null && !string.IsNullOrEmpty(jsonProduct.Seller.Name))
                        {
                            if (sellerCache.TryGetValue(jsonProduct.Seller.Name, out var cachedSellerId))
                            {
                                sellerId = cachedSellerId;
                            }
                            else
                            {
                                var existingSeller = await _context.Sellers
                                    .FirstOrDefaultAsync(s => s.Name == jsonProduct.Seller.Name);

                                if (existingSeller != null)
                                {
                                    sellerId = existingSeller.Id;
                                    sellerCache[jsonProduct.Seller.Name] = sellerId.Value;
                                    if (jsonProduct.Seller.Id > 0 && existingSeller.ExternalId == null)
                                    {
                                        existingSeller.ExternalId = jsonProduct.Seller.Id;
                                    }
                                }
                                else
                                {
                                    var newSeller = new Seller
                                    {
                                        Id = Guid.NewGuid(),
                                        ExternalId = jsonProduct.Seller.Id > 0 ? jsonProduct.Seller.Id : null,
                                        Name = jsonProduct.Seller.Name,
                                        IsOfficial = false
                                    };
                                    _context.Sellers.Add(newSeller);
                                    sellerId = newSeller.Id;
                                    sellerCache[jsonProduct.Seller.Name] = sellerId.Value;
                                }
                            }
                        }

                        var productCategories = new List<Guid>();
                        if (jsonProduct.Categories != null && jsonProduct.Categories.Any())
                        {
                            foreach (var cat in jsonProduct.Categories)
                            {
                                Guid catId;
                                
                                if (categoryCache.TryGetValue(cat.Name, out var cachedCatId))
                                {
                                    catId = cachedCatId;
                                }
                                else
                                {
                                    var existingCategory = await _context.Categories
                                        .FirstOrDefaultAsync(c => c.Name == cat.Name);
                                    
                                    if (existingCategory != null)
                                    {
                                        catId = existingCategory.Id;
                                        categoryCache[cat.Name] = catId;
                                    }
                                    else
                                    {
                                        var newCategory = new Category
                                        {
                                            Id = Guid.NewGuid(),
                                            ExternalId = negativeExternalId--,
                                            Name = cat.Name,
                                            Url = cat.Url,
                                            Level = 0
                                        };
                                        _context.Categories.Add(newCategory);
                                        catId = newCategory.Id;
                                        categoryCache[cat.Name] = catId;
                                    }
                                }
                                productCategories.Add(catId);
                            }
                        }

                        Guid mainCategoryId;
                        if (categoryCache.TryGetValue(categoryName, out var cachedMainCatId))
                        {
                            mainCategoryId = cachedMainCatId;
                        }
                        else
                        {
                            var mainCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);
                            if (mainCategory == null)
                            {
                                mainCategory = new Category
                                {
                                    Id = Guid.NewGuid(),
                                    ExternalId = negativeExternalId--,
                                    Name = categoryName,
                                    Level = 0
                                };
                                _context.Categories.Add(mainCategory);
                                mainCategoryId = mainCategory.Id;
                                categoryCache[categoryName] = mainCategoryId;
                            }
                            else
                            {
                                mainCategoryId = mainCategory.Id;
                                categoryCache[categoryName] = mainCategoryId;
                            }
                        }

                        var product = new CatalogProduct
                        {
                            Id = Guid.NewGuid(),
                            ExternalId = jsonProduct.Id,
                            No = $"TIKI-{jsonProduct.Id}",
                            Name = TruncateString(jsonProduct.Name, 250),
                            Summary = TruncateString(jsonProduct.ShortDescription ?? "", 500),
                            Description = jsonProduct.Description ?? "",
                            ShortDescription = TruncateString(jsonProduct.ShortDescription ?? "", 1000),
                            Price = jsonProduct.Price ?? 0,
                            OriginalPrice = jsonProduct.OriginalPrice,
                            RatingAverage = (decimal)(jsonProduct.RatingAverage ?? 0),
                            ReviewCount = jsonProduct.ReviewCount ?? 0,
                            AllTimeQuantitySold = jsonProduct.AllTimeQuantitySold ?? 0,
                            InventoryStatus = !string.IsNullOrEmpty(jsonProduct.InventoryStatus) ? jsonProduct.InventoryStatus : "IN_STOCK",
                            CategoryId = mainCategoryId,
                            BrandId = brandId,
                            SellerId = sellerId,
                            StockQuantity = 100
                        };

                        _context.Products.Add(product);

                        foreach (var catId in productCategories.Distinct())
                        {
                            _context.ProductCategories.Add(new ProductCategory
                            {
                                ProductId = product.Id,
                                CategoryId = catId
                            });
                        }

                        if (!productCategories.Contains(mainCategoryId))
                        {
                            _context.ProductCategories.Add(new ProductCategory
                            {
                                ProductId = product.Id,
                                CategoryId = mainCategoryId
                            });
                        }

                        if (jsonProduct.Images != null)
                        {
                            int position = 0;
                            foreach (var imageUrl in jsonProduct.Images)
                            {
                                _context.ProductImages.Add(new ProductImage
                                {
                                    Id = Guid.NewGuid(),
                                    ProductId = product.Id,
                                    Url = imageUrl,
                                    Position = position++,
                                    IsPrimary = position == 1
                                });
                            }
                        }

                        if (jsonProduct.Specifications != null)
                        {
                            foreach (var specGroup in jsonProduct.Specifications)
                            {
                                foreach (var spec in specGroup.Value)
                                {
                                    _context.ProductSpecifications.Add(new ProductSpecification
                                    {
                                        Id = Guid.NewGuid(),
                                        ProductId = product.Id,
                                        SpecGroup = TruncateString(specGroup.Key, 100),
                                        SpecName = TruncateString(spec.Key, 200),
                                        SpecValue = TruncateString(spec.Value, 1000)
                                    });
                                }
                            }
                        }

                        if (jsonProduct.Variants != null)
                        {
                            foreach (var variant in jsonProduct.Variants)
                            {
                                if (variant.Value != null)
                                {
                                    foreach (var value in variant.Value)
                                    {
                                        _context.ProductVariants.Add(new ProductVariant
                                        {
                                            Id = Guid.NewGuid(),
                                            ProductId = product.Id,
                                            AttributeName = TruncateString(variant.Key, 100),
                                            AttributeValue = TruncateString(value, 200)
                                        });
                                    }
                                }
                            }
                        }

                        totalProducts++;

                        if (totalProducts % 100 == 0)
                        {
                            await _context.SaveChangesAsync();
                            Console.WriteLine($"  Saved {totalProducts} products so far...");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Error processing product {jsonProduct.Id}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"Completed {fileName}. Total products: {totalProducts}");
            }

            if (_clipSearchService != null && totalProducts > 0)
            {
                Console.WriteLine($"\nIndexing {totalProducts} products to Elasticsearch in batches...");
                
                var allProducts = await _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Include(p => p.Specifications)
                    .ToListAsync();

                const int batchSize = 50;
                for (int i = 0; i < allProducts.Count; i += batchSize)
                {
                    var batch = allProducts.Skip(i).Take(batchSize).ToList();
                    await _clipSearchService.BulkIndexProductsAsync(batch);
                    Console.WriteLine($"  Indexed {Math.Min(i + batchSize, allProducts.Count)}/{allProducts.Count} products to Elasticsearch");
                }
                
                Console.WriteLine("Elasticsearch indexing completed!");
            }

            Console.WriteLine($"\nSeeding completed!");
            Console.WriteLine($"Total products imported: {totalProducts}");
            Console.WriteLine($"Skipped (already exist): {skippedProducts}");
        }

        private string TruncateString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }

    public class JsonProduct
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public decimal? Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string? Description { get; set; }
        public decimal? RatingAverage { get; set; }
        public int? ReviewCount { get; set; }
        public string? InventoryStatus { get; set; }
        public int? AllTimeQuantitySold { get; set; }
        public JsonBrand? Brand { get; set; }
        public JsonSeller? Seller { get; set; }
        public List<JsonCategory>? Categories { get; set; }
        public Dictionary<string, Dictionary<string, string>>? Specifications { get; set; }
        public List<string>? Images { get; set; }
        public Dictionary<string, List<string>>? Variants { get; set; }
        public int? QuantitySold { get; set; }
    }

    public class JsonBrand
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Slug { get; set; }
    }

    public class JsonSeller
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class JsonCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Url { get; set; }
    }
}

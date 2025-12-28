using Contracts.Domains;

namespace Product.API.Entities
{
    public class CatalogProduct : AuditableEntity<Guid>
    {
        public long ExternalId { get; set; } // Original Tiki product ID
        public string No { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public Guid CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public Guid? SellerId { get; set; }
        public decimal RatingAverage { get; set; }
        public int ReviewCount { get; set; }
        public int AllTimeQuantitySold { get; set; }
        public int QuantitySoldLast30Days { get; set; }
        public string InventoryStatus { get; set; } = "IN_STOCK"; // IN_STOCK, OUT_OF_STOCK, LOW_STOCK
        public int StockQuantity { get; set; }
        public string? Slug { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int? DiscountPercentage { get; set; }
        public Category Category { get; set; } = null!;
        public Brand? Brand { get; set; }
        public Seller? Seller { get; set; }
        public ICollection<ProductReview> Reviews { get; set; } = [];
        public ICollection<ProductImage> Images { get; set; } = [];
        public ICollection<ProductSpecification> Specifications { get; set; } = [];
        public ICollection<ProductVariant> Variants { get; set; } = [];
        public ICollection<ProductCategory> ProductCategories { get; set; } = [];
    }
}


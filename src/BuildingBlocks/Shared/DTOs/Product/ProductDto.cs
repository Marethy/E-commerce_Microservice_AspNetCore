namespace Shared.DTOs.Product
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string No { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int? DiscountPercentage { get; set; }
        public string? Slug { get; set; }

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public Guid? BrandId { get; set; }
        public string? BrandName { get; set; }

        public Guid? SellerId { get; set; }
        public string? SellerName { get; set; }

        // Rating & Reviews
        public decimal RatingAverage { get; set; }
        public int ReviewCount { get; set; }

        // Sales Statistics
        public int AllTimeQuantitySold { get; set; }
        public int QuantitySoldLast30Days { get; set; }

        // Inventory
        public string InventoryStatus { get; set; } = "IN_STOCK";
        public int StockQuantity { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // Related data
        public List<ProductImageDto>? Images { get; set; }
        public List<ProductSpecificationDto>? Specifications { get; set; }
    }

    public class ProductSummaryDto
    {
        public Guid Id { get; set; }
        public string No { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int? DiscountPercentage { get; set; }
        public string? Slug { get; set; }
        public decimal RatingAverage { get; set; }
        public int ReviewCount { get; set; }
        public int AllTimeQuantitySold { get; set; }
        public string InventoryStatus { get; set; } = "IN_STOCK";
        public string? PrimaryImageUrl { get; set; }
        public string? BrandName { get; set; }
    }
}
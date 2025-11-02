using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Product
{
    public class CreateProductDto 
    {
        [Required]
        [MaxLength(100)]
        public string No { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Summary { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? ShortDescription { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? OriginalPrice { get; set; }

        [Range(0, 100)]
        public int? DiscountPercentage { get; set; }

        [MaxLength(300)]
        public string? Slug { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        public Guid? BrandId { get; set; }
        
        public Guid? SellerId { get; set; }

        [MaxLength(50)]
        public string InventoryStatus { get; set; } = "IN_STOCK";

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public List<CreateProductImageDto>? Images { get; set; }
        public List<CreateProductSpecificationDto>? Specifications { get; set; }
    }
}
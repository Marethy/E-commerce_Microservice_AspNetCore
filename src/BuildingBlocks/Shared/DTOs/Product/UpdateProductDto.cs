using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Product
{
    public class UpdateProductDto 
    {
        [MaxLength(250)]
        public string? Name { get; set; }
        
        [MaxLength(500)]
        public string? Summary { get; set; }
        
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? ShortDescription { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? OriginalPrice { get; set; }

        [Range(0, 100)]
        public int? DiscountPercentage { get; set; }

        [MaxLength(300)]
        public string? Slug { get; set; }

        public Guid? CategoryId { get; set; }
        
        public Guid? BrandId { get; set; }
        
        public Guid? SellerId { get; set; }

        [MaxLength(50)]
        public string? InventoryStatus { get; set; }

        [Range(0, int.MaxValue)]
        public int? StockQuantity { get; set; }
    }
}
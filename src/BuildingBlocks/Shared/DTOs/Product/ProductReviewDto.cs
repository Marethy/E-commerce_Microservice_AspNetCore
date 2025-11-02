using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Product
{
    public class ProductReviewDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public string? Title { get; set; }
        public string? Comment { get; set; }
        public int HelpfulVotes { get; set; }
        public bool VerifiedPurchase { get; set; }
        public DateTimeOffset ReviewDate { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? LastModifiedDate { get; set; }
        public ProductDto? Product { get; set; }
    }

    public class CreateProductReviewDto
    {
        [Required]
        public Guid ProductId { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [Range(1.0, 5.0)]
        public decimal Rating { get; set; }
        
        [MaxLength(200)]
        public string? Title { get; set; }
        
        [MaxLength(2000)]
        public string? Comment { get; set; }
        
        public bool VerifiedPurchase { get; set; }
    }

    public class UpdateProductReviewDto
    {
        [Range(1.0, 5.0)]
        public decimal? Rating { get; set; }
        
        [MaxLength(200)]
        public string? Title { get; set; }
        
        [MaxLength(2000)]
        public string? Comment { get; set; }
    }
}
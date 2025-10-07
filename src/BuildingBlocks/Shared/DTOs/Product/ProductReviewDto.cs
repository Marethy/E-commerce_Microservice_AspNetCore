using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Product
{
    public class ProductReviewDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? LastModifiedDate { get; set; }
        public ProductDto? Product { get; set; }
    }

    public class CreateProductReviewDto
    {
        public Guid ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class UpdateProductReviewDto
    {
        public int? Rating { get; set; }
        public string? Comment { get; set; }
    }
}
using Contracts.Domains;

namespace Product.API.Entities
{
    public class ProductReview : AuditableEntity<Guid>
    {
        public Guid ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }

        // Navigation property
        public CatalogProduct Product { get; set; } = null!;
    }
}

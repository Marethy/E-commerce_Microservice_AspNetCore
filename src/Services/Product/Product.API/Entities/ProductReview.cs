using Contracts.Domains;

namespace Product.API.Entities
{
    public class ProductReview : AuditableEntity<Guid>
    {
        public Guid ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public string? Title { get; set; }
        public string? Comment { get; set; }
        public int HelpfulVotes { get; set; }
        public bool VerifiedPurchase { get; set; }
        public DateTimeOffset ReviewDate { get; set; }

        public CatalogProduct Product { get; set; } = null!;
    }
}

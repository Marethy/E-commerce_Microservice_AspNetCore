using Contracts.Domains;

namespace Product.API.Entities
{
    public class CatalogProduct : AuditableEntity<Guid>
    {
        public string No { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public Guid CategoryId { get; set; }

        // Navigation properties
        public Category Category { get; set; } = null!;
        public ICollection<ProductReview> Reviews { get; set; } = [];
    }
}
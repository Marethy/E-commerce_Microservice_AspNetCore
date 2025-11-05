using Contracts.Domains;

namespace Product.API.Entities
{
    /// <summary>
    /// Represents product images
    /// </summary>
    public class ProductImage : AuditableEntity<Guid>
    {
        public Guid ProductId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public int Position { get; set; }
        public bool IsPrimary { get; set; }

        // Navigation property
        public CatalogProduct Product { get; set; } = null!;
    }
}

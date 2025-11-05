using Contracts.Domains;

namespace Product.API.Entities
{
    /// <summary>
    /// Represents a product brand/manufacturer
    /// </summary>
    public class Brand : AuditableEntity<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? CountryOfOrigin { get; set; }
        public string? LogoUrl { get; set; }
        public string? Description { get; set; }

        // Navigation properties
        public ICollection<CatalogProduct> Products { get; set; } = [];
    }
}

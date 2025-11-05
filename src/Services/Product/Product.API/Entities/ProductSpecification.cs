using Contracts.Domains;

namespace Product.API.Entities
{
    /// <summary>
    /// Represents product technical specifications
    /// </summary>
    public class ProductSpecification : AuditableEntity<Guid>
    {
        public Guid ProductId { get; set; }
        public string SpecGroup { get; set; } = string.Empty;
        public string SpecName { get; set; } = string.Empty;
        public string SpecValue { get; set; } = string.Empty;

        // Navigation property
        public CatalogProduct Product { get; set; } = null!;
    }
}

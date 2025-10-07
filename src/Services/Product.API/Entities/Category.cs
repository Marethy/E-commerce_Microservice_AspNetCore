using Contracts.Domains;

namespace Product.API.Entities
{
    public class Category : EntityBase<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation property
        public ICollection<CatalogProduct> Products { get; set; } = [];
    }
}

using Contracts.Domains;

namespace Product.API.Entities
{
    public class Category : AuditableEntity<Guid>
    {
        public int ExternalId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Url { get; set; }
        public Guid? ParentId { get; set; }
        public int Level { get; set; }
        public Category? Parent { get; set; }
        public ICollection<Category> Children { get; set; } = [];
        public ICollection<CatalogProduct> Products { get; set; } = [];
        public ICollection<ProductCategory> ProductCategories { get; set; } = [];
    }
}

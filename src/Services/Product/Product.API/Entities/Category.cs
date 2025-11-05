using Contracts.Domains;

namespace Product.API.Entities
{
    public class Category : AuditableEntity<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Url { get; set; }
        public Guid? ParentId { get; set; }
        public int Level { get; set; }

        // Navigation properties
        public Category? Parent { get; set; }
        public ICollection<Category> Children { get; set; } = [];
        public ICollection<CatalogProduct> Products { get; set; } = [];
        
        // Many-to-many with Product
        public ICollection<ProductCategory> ProductCategories { get; set; } = [];
    }
}

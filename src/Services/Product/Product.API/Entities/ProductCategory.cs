using Contracts.Domains;

namespace Product.API.Entities
{
    public class ProductCategory : EntityBase<int>
    {
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }

        public CatalogProduct Product { get; set; } = null!;
        public Category Category { get; set; } = null!;
    }
}

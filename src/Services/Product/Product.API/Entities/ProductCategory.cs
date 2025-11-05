using System.ComponentModel.DataAnnotations;

namespace Product.API.Entities
{
    /// <summary>
    /// Join table for many-to-many relationship between Product and Category
    /// </summary>
    public class ProductCategory
    {
        [Required]
        public Guid ProductId { get; set; }
        
        [Required]
        public Guid CategoryId { get; set; }

        // Navigation properties
        public CatalogProduct Product { get; set; } = null!;
        public Category Category { get; set; } = null!;
    }
}

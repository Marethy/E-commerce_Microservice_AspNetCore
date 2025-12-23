using Contracts.Domains;

namespace Product.API.Entities
{
    /// <summary>
    /// Represents product variants (color, size, material, etc.)
    /// For example: Màu: Đỏ, Kích cỡ: XL, Họa Tiết: Gấu nâu
    /// </summary>
    public class ProductVariant : AuditableEntity<Guid>
    {
        public Guid ProductId { get; set; }
        public string AttributeName { get; set; } = string.Empty; // e.g., "Màu", "Kích cỡ", "Họa Tiết"
        public string AttributeValue { get; set; } = string.Empty; // e.g., "Đỏ", "XL", "Gấu nâu"

        // Navigation property
        public CatalogProduct Product { get; set; } = null!;
    }
}

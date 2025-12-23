using Contracts.Domains;

namespace Product.API.Entities
{
    /// <summary>
    /// Represents a seller/merchant
    /// </summary>
    public class Seller : AuditableEntity<Guid>
    {
        public int? ExternalId { get; set; } // Original Tiki seller ID
        public string Name { get; set; } = string.Empty;
        public bool IsOfficial { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public decimal Rating { get; set; }
        public int TotalSales { get; set; }

        // Navigation properties
        public ICollection<CatalogProduct> Products { get; set; } = [];
    }
}

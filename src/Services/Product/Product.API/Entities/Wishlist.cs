using Contracts.Domains;

namespace Product.API.Entities;

/// <summary>
/// Represents a user's wishlist item
/// </summary>
public class Wishlist : AuditableEntity<Guid>
{
    /// <summary>
    /// User ID from Identity Service
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Product ID
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Price when added to wishlist (for price tracking)
    /// </summary>
    public decimal OriginalPrice { get; set; }

    /// <summary>
    /// Date when added to wishlist
    /// </summary>
  public DateTimeOffset AddedDate { get; set; }

    // Navigation properties
  public CatalogProduct Product { get; set; } = null!;
}

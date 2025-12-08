using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Product;

public class WishlistItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductNo { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public string? PrimaryImageUrl { get; set; }
  public DateTimeOffset AddedDate { get; set; }
    public bool IsInStock { get; set; }
    public string InventoryStatus { get; set; } = string.Empty;
    public decimal? CurrentPrice { get; set; }
    public bool PriceChanged { get; set; }
    public decimal? PriceDropPercentage { get; set; }
    public string? BrandName { get; set; }
    public decimal RatingAverage { get; set; }
    public int ReviewCount { get; set; }
}

public class WishlistDto
{
    public List<WishlistItemDto> Items { get; set; } = new();
    public int Total { get; set; }
  public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages { get; set; }
}

public class AddToWishlistDto
{
    [Required]
    public Guid ProductId { get; set; }
}

public class WishlistCountDto
{
    public int Count { get; set; }
}

public class WishlistStatusDto
{
    public bool IsInWishlist { get; set; }
}

public class WishlistAnalyticsDto
{
    public int Total { get; set; }
    public int Recent { get; set; } // Added in last 7 days
}

public class MostWishlistedProductDto
{
    public Guid ProductId { get; set; }
    public string ProductNo { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int WishlistCount { get; set; }
    public decimal Price { get; set; }
    public string? PrimaryImageUrl { get; set; }
}

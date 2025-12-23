namespace Shared.DTOs.Product
{
    /// <summary>
    /// Filter DTO for product search with multiple criteria
    /// </summary>
    public class ProductFilterDto
    {
        public string? Q { get; set; } // Search keyword
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinRating { get; set; }
        public decimal? MaxRating { get; set; }
        public List<Guid>? BrandIds { get; set; }
        public List<string>? BrandNames { get; set; }
        public List<Guid>? CategoryIds { get; set; }
        public List<Guid>? ProductIds { get; set; }
        public string? InventoryStatus { get; set; }
        public string? SortBy { get; set; } // "price", "rating", "sales", "created"
        public string? SortDirection { get; set; } // "asc", "desc"
    }
}

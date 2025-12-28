namespace Shared.DTOs.Product
{
    /// <summary>
    /// Search request DTO for product search with text and/or image
    /// </summary>
    public class ProductSearchRequestDto
    {
        public string? Query { get; set; } // Text search query
        public string? ImageBase64 { get; set; } // Base64 encoded image for visual search
        public ProductFilterDto? Filter { get; set; } // Additional filters
        public int Page { get; set; } = 0;
        public int Size { get; set; } = 20;
    }
}

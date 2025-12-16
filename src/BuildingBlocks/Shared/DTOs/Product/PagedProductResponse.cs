namespace Shared.DTOs.Product
{
    public class PagedProductResponse
    {
        public List<ProductDto> Content { get; set; } = new();
        public PageMetadata Meta { get; set; } = new();
    }

    public class PageMetadata
    {
        public int Page { get; set; }
        public int Size { get; set; }
        public int TotalElements { get; set; }
        public int TotalPages { get; set; }
        public bool Last { get; set; }
    }
}

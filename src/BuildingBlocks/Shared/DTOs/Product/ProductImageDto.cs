namespace Shared.DTOs.Product
{
    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public int Position { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class CreateProductImageDto
    {
        public string Url { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public int Position { get; set; }
        public bool IsPrimary { get; set; }
    }
}

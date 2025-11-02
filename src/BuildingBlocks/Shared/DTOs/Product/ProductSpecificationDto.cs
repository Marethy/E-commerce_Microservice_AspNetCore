namespace Shared.DTOs.Product
{
    public class ProductSpecificationDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string SpecGroup { get; set; } = string.Empty;
        public string SpecName { get; set; } = string.Empty;
        public string SpecValue { get; set; } = string.Empty;
    }

    public class CreateProductSpecificationDto
    {
        public string SpecGroup { get; set; } = string.Empty;
        public string SpecName { get; set; } = string.Empty;
        public string SpecValue { get; set; } = string.Empty;
    }
}

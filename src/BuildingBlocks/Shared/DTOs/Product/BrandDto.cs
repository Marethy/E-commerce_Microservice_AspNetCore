namespace Shared.DTOs.Product
{
    public class BrandDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? CountryOfOrigin { get; set; }
        public string? LogoUrl { get; set; }
        public string? Description { get; set; }
    }

    public class CreateBrandDto
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? CountryOfOrigin { get; set; }
        public string? LogoUrl { get; set; }
        public string? Description { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Product
{
    public class CreateProductDto 
    {
        [Required]
        public string No { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public Guid CategoryId { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Product
{
    public abstract class CreateOrUpdateProductDto
    {
        [Required]
        [StringLength(250)]
        public string Name { get; set; }

        public string Summary { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "The price must be greater than 0.")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal Price { get; set; }
    }
}
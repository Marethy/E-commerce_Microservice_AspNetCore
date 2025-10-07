using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Product
{
    public class UpdateProductDto 
    {
        public string? Name { get; set; }
        public string? Summary { get; set; }
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        public Guid? CategoryId { get; set; }
    }
}
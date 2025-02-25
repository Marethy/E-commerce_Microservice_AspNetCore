using Contracts.Domains;
using System.ComponentModel.DataAnnotations;

namespace Product.API.Entities
{
    public class CatalogProduct:EntityAuditBase<long>
    {

        [Required]
        [StringLength(100)]
        public string No { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Name { get; set; } = string.Empty;

        [StringLength(int.MaxValue)]
        public string Summary { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

   //     public int StockQuanlitity { get; set; }    
    }
}


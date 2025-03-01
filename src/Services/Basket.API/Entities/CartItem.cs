using System.ComponentModel.DataAnnotations;

namespace Basket.API.Entities
{
    public class CartItem
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        public string ItemNo { get; set; }

        [Required]
        [StringLength(250)]
        public string ItemName { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Item price must be greater than 0.")]
        public decimal ItemPrice { get; set; }
    }
}



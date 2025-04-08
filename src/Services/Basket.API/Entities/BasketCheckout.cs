using System.ComponentModel.DataAnnotations;

namespace Basket.API.Entities
{
    public class BasketCheckout
    {
        public required string Username { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than 0.")]
        public decimal TotalPrice { get; set; }

        [Required]
        [StringLength(250)]
        public required  string FirstName { get; set; }

        [Required]
        [StringLength(250)]
        public required  string LastName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}

